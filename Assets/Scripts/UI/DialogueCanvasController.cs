using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WordsOnTheWaves.Events;
using WordsOnTheWaves.Data;
using WordsOnTheWaves.Core;
using WordsOnTheWaves.NPCs;
using TMPro;

namespace WordsOnTheWaves.UI
{
    public class DialogueCanvasController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _canvasRoot;
        [SerializeField] private TMP_Text _dialogueText;
        [SerializeField] private Button _screenTapButton; // Nút vô hình toàn màn hình để chuyển thoại
        
        [Header("Book Selection")]
        [SerializeField] private GameObject _buttonsGroup;
        [SerializeField] private Button[] _bookButtons = new Button[3];
        
        [Header("Book Sprites (Mapping by Genre)")]
        [Tooltip("Kéo thả hình ảnh sách theo đúng thứ tự của BookGenre (Crime=0, Drama=1, Fact=2, Fantasy=3, Classic=4, Kids=5, Travel=6)")]
        [SerializeField] private Sprite[] _genreSprites = new Sprite[7];

        private CustomerController _currentCustomer;
        private BookGenre _requestedGenre;
        private CustomerType _customerType;
        
        private int _currentPhase = 0; // 0: Idle, 1: Greeting, 2: Hint/Buttons, 3: Response, 4: Leaving
        
        // Dữ liệu tạm thời
        private string[] _buttonBookIds = new string[3];
        private bool _selectedBookIsCorrect;
        private string _selectedBookIdToSell;

        private void OnEnable()
        {
            EventManager.OnCustomerNeedsAdvice += HandleCustomerNeedsAdvice;
            EventManager.OnCustomerLeftCounter += HandleCustomerLeftCounter;
            
            if (_screenTapButton != null)
                _screenTapButton.onClick.AddListener(OnScreenTapped);
                
            for (int i = 0; i < _bookButtons.Length; i++)
            {
                int index = i; // local copy for closure
                if (_bookButtons[i] != null)
                    _bookButtons[i].onClick.AddListener(() => OnBookSelected(index));
            }
        }

        private void OnDisable()
        {
            EventManager.OnCustomerNeedsAdvice -= HandleCustomerNeedsAdvice;
            EventManager.OnCustomerLeftCounter -= HandleCustomerLeftCounter;
            
            if (_screenTapButton != null)
                _screenTapButton.onClick.RemoveListener(OnScreenTapped);
                
            for (int i = 0; i < _bookButtons.Length; i++)
            {
                if (_bookButtons[i] != null)
                    _bookButtons[i].onClick.RemoveAllListeners();
            }
        }

        private void Start()
        {
            if (_canvasRoot != null) _canvasRoot.SetActive(false);
        }

        private void HandleCustomerNeedsAdvice(CustomerController customer, BookGenre requestedGenre, CustomerType type)
        {
            _currentCustomer = customer;
            _requestedGenre = requestedGenre;
            _customerType = type;
            
            if (_canvasRoot != null) _canvasRoot.SetActive(true);
            if (_buttonsGroup != null) _buttonsGroup.SetActive(false);
            
            // Phase 1: Greeting
            _currentPhase = 1;
            string typeStr = type == CustomerType.Special ? "Special" : "NeedAdvice";
            if (_dialogueText != null)
                _dialogueText.text = DataManager.Instance.GetRandomDialogue("greetings", typeStr);
        }

        private void OnScreenTapped()
        {
            if (_currentPhase == 1)
            {
                // Chuyển sang Phase 2: Requirement
                _currentPhase = 2;
                if (_dialogueText != null)
                    _dialogueText.text = DataManager.Instance.GetRandomDialogue("genreHints", _requestedGenre.ToString());
                ShowBookButtons();
            }
            else if (_currentPhase == 3)
            {
                // Người chơi đã đọc xong câu cảm thán, tap để khách lấy sách và rời đi
                FinishAdviceAndLetCustomerGo();
            }
        }

        private void ShowBookButtons()
        {
            if (_buttonsGroup != null) _buttonsGroup.SetActive(true);
            
            // Tìm 3 sách (Đúng, Gần Đúng, Sai)
            var wagonCargo = DataManager.Instance.GetWagonCargo();
            List<BookData> wagonBooks = new List<BookData>();
            foreach (var entry in wagonCargo)
            {
                if (entry.Value > 0 && DataManager.Instance.Books.TryGetValue(entry.Key, out var bookData))
                {
                    wagonBooks.Add(bookData);
                }
            }

            BookData correctBook = null;
            BookData nearBook = null;
            BookData wrongBook = null;
            
            var nearGenres = DataManager.Instance.GetNearGenres(_requestedGenre);

            // Tìm sách đúng và gần đúng trên xe
            foreach (var book in wagonBooks)
            {
                if (correctBook == null && book.genre == _requestedGenre) correctBook = book;
                else if (nearBook == null && nearGenres.Contains(book.genre)) nearBook = book;
            }

            // Tìm sách sai (trên xe, hoặc mượn từ tổng nếu xe ko có)
            foreach (var book in wagonBooks)
            {
                if (book != correctBook && book != nearBook && book.genre != _requestedGenre && !nearGenres.Contains(book.genre))
                {
                    wrongBook = book;
                    break;
                }
            }

            // Nếu vẫn thiếu sách, lấy random từ kho tổng làm chim mồi
            var allBooks = new List<BookData>(DataManager.Instance.Books.Values);
            
            if (correctBook == null) correctBook = GetFallbackBook(allBooks, _requestedGenre, null, true);
            if (nearBook == null) 
            {
                if (nearGenres.Count > 0) nearBook = GetFallbackBook(allBooks, nearGenres[0], null, true);
                else nearBook = GetFallbackBook(allBooks, _requestedGenre, null, false); // fallback to wrong
            }
            if (wrongBook == null) wrongBook = GetFallbackBook(allBooks, _requestedGenre, nearGenres, false);

            // Gán vào 3 nút ngẫu nhiên
            List<BookData> options = new List<BookData> { correctBook, nearBook, wrongBook };
            Shuffle(options);

            for (int i = 0; i < 3; i++)
            {
                _buttonBookIds[i] = options[i].id;
                
                // Cập nhật Texture/Sprite cho nút tương ứng với loại sách
                var image = _bookButtons[i].GetComponent<Image>();
                if (image != null && _genreSprites != null && (int)options[i].genre < _genreSprites.Length)
                {
                    image.sprite = _genreSprites[(int)options[i].genre];
                }
            }
        }

        private BookData GetFallbackBook(List<BookData> allBooks, BookGenre targetGenre, List<BookGenre> nearGenres, bool wantsMatch)
        {
            foreach (var book in allBooks)
            {
                if (wantsMatch && book.genre == targetGenre) return book;
                if (!wantsMatch && book.genre != targetGenre && (nearGenres == null || !nearGenres.Contains(book.genre))) return book;
            }
            return allBooks[0];
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int k = Random.Range(0, i + 1);
                T value = list[k];
                list[k] = list[i];
                list[i] = value;
            }
        }

        private void OnBookSelected(int buttonIndex)
        {
            if (_currentPhase != 2) return; // Chỉ cho bấm lúc phase 2
            
            if (_buttonsGroup != null) _buttonsGroup.SetActive(false); // Ẩn ngay lập tức
            _currentPhase = 3;

            string selectedBookId = _buttonBookIds[buttonIndex];
            if (!DataManager.Instance.Books.TryGetValue(selectedBookId, out var bookData)) return;

            bool isCorrect = (bookData.genre == _requestedGenre);
            bool isNear = DataManager.Instance.GetNearGenres(_requestedGenre).Contains(bookData.genre);

            _selectedBookIsCorrect = (isCorrect || isNear);
            _selectedBookIdToSell = selectedBookId;

            if (_selectedBookIsCorrect)
            {
                // Thành công - Mới chỉ đổi text, KHÔNG cho khách rời đi ngay
                if (_dialogueText != null) _dialogueText.text = DataManager.Instance.GetRandomDialogue("responses", "accept");
            }
            else
            {
                // Thất bại - Mới chỉ đổi text, KHÔNG cho khách rời đi ngay
                if (_dialogueText != null) _dialogueText.text = DataManager.Instance.GetRandomDialogue("responses", "reject");
            }
        }

        private void FinishAdviceAndLetCustomerGo()
        {
            if (_currentPhase == 4) return;
            _currentPhase = 4;

            if (_selectedBookIsCorrect)
            {
                // Thực sự bán sách
                DataManager.Instance.SellBookFromWagon(_selectedBookIdToSell);

                // Tính tiền
                if (DataManager.Instance.Books.TryGetValue(_selectedBookIdToSell, out var bookData))
                {
                    int sellPrice = bookData.sellPrice;
                    float multiplier = 1f;
                    
                    var rules = DataManager.Instance.CustomerSpawnRules;
                    foreach (var rule in rules)
                    {
                        if (rule.customerType == _customerType)
                        {
                            multiplier = rule.bonusMultiplier;
                            break;
                        }
                    }

                    int bonusFlat = DataManager.Instance.CoinConfig != null ? DataManager.Instance.CoinConfig.adviceBonusFlat : 0;
                    int totalCoins = Mathf.RoundToInt(sellPrice * multiplier) + bonusFlat;
                    
                    DataManager.Instance.AddCoins(totalCoins);
                }

                EventManager.OnAdviceFinished?.Invoke(_currentCustomer, true);
                _currentCustomer.OnAdviceReceived(true);
            }
            else
            {
                // Khách giận dỗi rời đi
                EventManager.OnAdviceFinished?.Invoke(_currentCustomer, false);
                _currentCustomer.OnAdviceReceived(false);
            }
        }

        private void HandleCustomerLeftCounter(GameObject customerObj)
        {
            var customer = customerObj.GetComponent<CustomerController>();
            if (customer != null && customer == _currentCustomer)
            {
                CloseDialogue();
            }
        }

        private void CloseDialogue()
        {
            _currentPhase = 0;
            _currentCustomer = null;
            if (_canvasRoot != null) _canvasRoot.SetActive(false);
        }
    }
}
