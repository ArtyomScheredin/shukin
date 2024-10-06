using System.Reflection;

namespace CardFool
{
    internal class MPlayer1
    {
        private readonly string _name = "Player1";
        private readonly List<SCard> _unsortedHand = new List<SCard>();
        private SortedSet<SCard>? _hand; // карты на руке

        // Возвращает имя игрока
        //CPU O(1) MEM O(1)
        public string GetName()
        {
            return _name;
        }

        // количество карт на руке
        //CPU O(1) MEM O(1)
        public int GetCount()
        {
            return _hand.Count;
        }

        // Добавляет новую карту в руку
        //CPU O(log(N)) MEM O(1)
        public void AddToHand(SCard card)
        {
            if (_hand == null)
            {
                _unsortedHand.Add(card);
            }
            else
            {
                _hand.Add(card);
            }
        }

        // Сделать ход (первый)
        //Ходим минимальными картами
        //Худший случай: CPU O(N) MEM O(M)
        //M - количество мастей, N - число карт на руках у игрока
        public List<SCard> LayCards()
        {
            SortHandIfNotSorted();
            List<SCard> smallestCards = new List<SCard>();
            foreach (SCard sCard in _hand)
            {
                if (smallestCards.Count == 0 || sCard.Rank == smallestCards[smallestCards.Count - 1].Rank)
                {
                    smallestCards.Add(sCard);
                }
            }

            foreach (SCard card in smallestCards)
            {
                _hand.Remove(card); //log(N)
            }

            return smallestCards;
        }

        //Худший случай: CPU O(N * log (N)) MEM O(K), K - число карт на столе,
        //N - число карт на руках у игрока
        public bool AddCards(List<SCardPair> table)
        {
            SortHandIfNotSorted();
            HashSet<int> ranksInTable = [];
            foreach (SCardPair sCardPair in table)
            {
                ranksInTable.Add(sCardPair.Up.Rank);
            }
            
            if (table.Count != MTable.TotalCards)
            {
                return false;
            }

            int cardsToAddLeft = MTable.TotalCards - table.Count;
            List<SCard> addedCards = new List<SCard>();
            foreach (SCard sCard in _hand)
            {
                if (ranksInTable.Contains(sCard.Rank))
                {
                    if (cardsToAddLeft-- == 0)
                    {
                        break;
                    }
                    addedCards.Add(sCard);
                }
            }
            if (addedCards.Count == 0)
            {
                return false;
            }
            
            foreach (var addedCard in addedCards)
            {
                table.Add(new SCardPair(addedCard));
                _hand.Remove(addedCard); //log(N)
            }

            return true;
        }

        // Отбиться.
        // На вход подается набор карт на столе, часть из них могут быть уже покрыты
        // Худший случай: CPU O(K * log (N) + K * N) MEM O(N),
        // K - число карт на столе,
        // N - число карт на руках у игрока
        public bool Defend(List<SCardPair> table)
        {
            SortHandIfNotSorted();
            for (int index = 0; index < table.Count; index++)
            {
                SCardPair sCardPair = table[index];
                if (sCardPair.Beaten)
                {
                    continue;
                }

                foreach (SCard sCard in _hand)
                {
                    if (sCardPair.SetUp(sCard, MTable.GetTrump().Suit))
                    {
                        table[index] = sCardPair;
                        break;
                    }
                }

                if (!sCardPair.Beaten)
                {
                    return false;
                }
                _hand.Remove(sCardPair.Up);
            }

            return true;
        }
        
        // Вывести в консоль карты на руке
        public void ShowHand()
        {
            SortHandIfNotSorted();
            Console.WriteLine("Hand " + _name);
            foreach (SCard card in _hand)
            {
                MTable.ShowCard(card);
                Console.Write(MTable.Separator);
            }

            Console.WriteLine();
        }

        private void SortHandIfNotSorted()
        {
            if (MTable.GetTrump() != null && _hand == null)
            {
                _hand = new SortedSet<SCard>(new CardComparer(MTable.GetTrump().Suit));
                foreach (SCard card in _unsortedHand)
                {
                    _hand.Add(card);
                }
            }
        }   
        
        private class CardComparer(Suits trumpSuit) : IComparer<SCard>
        {
            public int Compare(SCard card1, SCard card2)
            {
                bool card1IsTrump = card1.Suit == trumpSuit;
                bool card2IsTrump = card2.Suit == trumpSuit;

                if (card1IsTrump && !card2IsTrump)
                    return 1;
                if (!card1IsTrump && card2IsTrump)
                    return -1;

                var compareRanks = card1.Rank.CompareTo(card2.Rank);
                if (compareRanks != 0)
                {
                    return compareRanks;
                }

                return card1.Suit.CompareTo(card2.Suit);
            }
        }
    }
}