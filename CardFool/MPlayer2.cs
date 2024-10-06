using System.Reflection;

namespace CardFool;

internal class MPlayer2
    {
        private int GetOpponentCards()
        {
            int size;
            var type = typeof(MTable);
            //На случай, если содержимое модуля будет скопированно в другой класс, осуществляем проверку
            if (this is MPlayer2)
            {
                var field = type?.GetField("plHand1", BindingFlags.NonPublic | BindingFlags.Static);
                List<SCard>? value = (List<SCard>)field?.GetValue(null);
                size = value.Count;
            }
            else
            {
                var field = type?.GetField("plHand2", BindingFlags.NonPublic | BindingFlags.Static);
                List<SCard>? value = (List<SCard>)field?.GetValue(null);
                size = value.Count;
            }
            return size;
        }


        private int getDeckSize()
        {
            var type = typeof(MTable);
            var field = type?.GetField("deck", BindingFlags.NonPublic | BindingFlags.Static);
            List<SCard>? value = (List<SCard>) field?.GetValue(null);
            int size = value.Count;

            return size;
        }

        private const string _name = "ryanna";
        private List<SCard> _hand = new List<SCard>(); // карты на руке
        private Random _random = new Random();
        private static Suits _trump = MTable.GetTrump().Suit;

        // Возвращает имя игрока
        public string GetName()
        {
            return _name;
        }

        // количество карт на руке
        public int GetCount()
        {
            return _hand.Count();
        }
        // Добавляет новую карту в руку
        public void AddToHand(SCard card)
        {
            _hand.Add(card);
        }

        public int Choose()
        {
            return _random.Next(0, 2);
        }

        // Сделать ход (первый)
        //Ходим минимальными картами
        public List<SCard> LayCards()
        {
            //Сount - переменная для счета атакующих карт
            int count = 1;
            //Создаем список, в который будем помещать все атакующие карты, если карт для атаки нет -возвращается пустой список
            List<SCard> cards = new List<SCard>();
           
            //Ищем минимальный индекс текущих карт в руке для атаки
            int minCardIndToAttack = FindMinIndexToAttack();
            for (int i = 0; i < _hand.Count; i++)
            {
                //Если размер колоды больше восьми, не кладем в список козырные карты
                if (getDeckSize() > 10)
                {
                    if ((_hand[i].Rank == _hand[minCardIndToAttack].Rank) && (_hand[i].Suit != _trump))
                    {
                        //Проверяем, не больше ли 6 карт в списке атакующих и не больше ли их, чем есть у оппонента
                        if ((count + 1 <= GetOpponentCards()) && (count + 1 <= 6))
                        {
                            cards.Add(_hand[i]);
                            count++;
                        }
                        //Если да, то прерываем перебор
                        else break;
                    }
                }
                //Если в колоде меньше восьми карт, то кладем в список атакующих карт и козырные тоже
                else
                {
                    if (_hand[i].Rank == _hand[minCardIndToAttack].Rank)
                    {
                        if (count + 1 <= GetOpponentCards() && (count + 1 <= 6))
                        {
                            cards.Add(_hand[i]);
                            count++;
                        }
                        //Если да, то прерываем перебор
                        else break;
                    }
                }
            }

            if (cards.Contains(_hand[minCardIndToAttack]) == false)
            {
                cards.Add(_hand[minCardIndToAttack]);
            }

            //Убираем все атакующие карты из руки
            foreach (SCard card in cards)
            {
                if (_hand.Contains(card))
                {
                    _hand.Remove(card);
                }
            }
            return cards;
        }

        // Отбиться.
        // На вход подается набор карт на столе, часть из них могут быть уже покрыты
        public bool Defend(List<SCardPair> table)
        {
            //Выбираем стратегию
            int strategy = Choose();
            //Вариант защиты при первой стратегии
            if (strategy == 0)
            {
                // —>| Пока в колоде больше 8 карт - принимаем карты |<— \\
                if (getDeckSize() > 8)
                {
                    return false;
                }
                // —>| Если в колоде меньше 10 карт, отбиваемся наименьшей картой |<— \\
                else
                {
                    for (int i = 0; i < table.Count; i++)
                    {
                        if (!table[i].Beaten)
                        {
                            int minCardIndToDefend = FindMinIndexToDefend(table[i].Down);
                            if (minCardIndToDefend >= 0) // Предполагается, что возвращаемый индекс >= 0
                            {
                                SCardPair temp = table[i];
                                SCard defendCard = _hand[minCardIndToDefend];
                                temp.SetUp(defendCard, _trump);
                                table[i] = temp;
                                _hand.Remove(defendCard);
                            }
                            else
                            {
                                return false; // Если нет карт для защиты, заканчиваем
                            }
                        }
                    }
                }
            }
            //Вариант защиты при второй стратегии
            else
            {
                //Если в колоде больше восьми карт, то отбиваемся картой, разница цены которых не больше четрех
                if (getDeckSize() > 8)
                {
                    for (int i = 0; i < table.Count; i++)
                    {
                        if (!table[i].Beaten)
                        {
                            int minCardIndToDefend = FindMinIndexToDefend(table[i].Down);
                            if (minCardIndToDefend >= 0 && (_hand[minCardIndToDefend].Rank -
                           table[i].Down.Rank < 8))
                            {
                                SCardPair temp = table[i];
                                SCard defendCard = _hand[minCardIndToDefend];
                                temp.SetUp(defendCard, _trump);
                                table[i] = temp;
                                _hand.Remove(defendCard);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
                //В противном случае отбиваемся всеми возможными способами
                else
                {
                    for (int i = 0; i < table.Count; i++)
                    {
                        if (!table[i].Beaten)
                        {
                            int minCardIndToDefend = FindMinIndexToDefend(table[i].Down);
                            if (minCardIndToDefend >= 0) // Предполагается, что возвращаемый индекс >= 0
                            {
                                SCardPair temp = table[i];
                                SCard defendCard = _hand[minCardIndToDefend];
                                temp.SetUp(defendCard, _trump);
                                table[i] = temp;
                                _hand.Remove(defendCard);
                            }
                            else
                            {
                                return false; // Если нет карт для защиты, заканчиваем
                            }
                        }
                    }
                }
            }
            return true;
        }


        public int FindMinIndexToDefend(SCard attackCard)
        {
            int minInd = -1; // Начальное значение индекса
            for (int i = 0; i < _hand.Count; i++)
            {
                bool isTrumpCard = _hand[i].Suit == _trump;
                bool isDefendableCard = false;
                if (attackCard.Suit == _trump)
                {
                    // Если атакующая карта - козырь, защищающая карта должна быть козырной и старше
                    if (isTrumpCard && _hand[i].Rank > attackCard.Rank)
                    {
                        isDefendableCard = true;
                    }
                }
                else
                {
                    // Если атакующая карта не козырь, защищающая карта либо старше по рангу и той же масти, либо козырная
                    if (((_hand[i].Suit == attackCard.Suit) && (_hand[i].Rank > attackCard.Rank)) || isTrumpCard)
                    {
                        isDefendableCard = true;
                    }
                }
                if (isDefendableCard && (minInd == -1 || _hand[i].Rank < _hand[minInd].Rank))
                {
                    minInd = i; // Обновляем индекс минимальной защитной карты
                }
            }
            return minInd;
        }

        public int FindMinIndexToAttack()
        {
            int minInd = -1;
            int minRank = int.MaxValue; // Используем максимальное значение для поиска минимума
           
            for (int i = 0; i < _hand.Count; i++)
            {
                if (_hand[i].Suit != _trump)
                {
                    if (_hand[i].Rank < minRank)
                    {
                        minInd = i;
                        minRank = _hand[i].Rank;
                    }
                }
            }
            //Если все карты козырные, ищем минимальную карту среди козырных
            if(minRank == int.MaxValue)
            {
                for(int i = 0; i < _hand.Count; i++)
                {
                    if (_hand[i].Rank < minRank)
                    {
                        minInd = i;
                        minRank = _hand[i].Rank;
                    }
                }
            }
            return minInd; // Вернем -1, если карта не найдена(только в случае, если карт нет),или индекс найденной карты
        }

        public bool AddCards(List<SCardPair> table)
        {
            // Переменная для отслеживания результата
            bool result = false;
            // Проверяем, достаточно ли карт в колоде
            if (getDeckSize() > 8) return false;
            // Создаем HashSet для быстрого поиска по рангам карт на столе
            HashSet<int> ranksInTable = new HashSet<int>(table.Select(pair => pair.Up.Rank));
            int count = 0;
            for (int i = 0; i < GetCount(); i++)
            {
                if ((GetOpponentCards() > 0) && (count <= 6))
                {
                    // Проверяем, есть ли ранг карты на руке в столе
                    if (ranksInTable.Contains(_hand[i].Rank))
                    {
                        result = true;
                        count++;
                        SCardPair sCardPair = new SCardPair();
                        sCardPair.Down = _hand[i];
                        table.Add(sCardPair);
                        _hand.Remove(_hand[i]);

                    }
                }
                else break;
            }
            return result;
        }

        // Вывести в консоль карты на руке
        public void ShowHand()
        {
            Console.WriteLine("Hand " + _name);
            foreach (SCard card in _hand)
            {
                MTable.ShowCard(card);
                Console.Write(MTable.Separator);
            }
            Console.WriteLine();
        }
    }