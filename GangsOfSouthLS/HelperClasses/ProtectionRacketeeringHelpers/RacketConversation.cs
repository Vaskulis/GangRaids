using GangsOfSouthLS.HelperClasses.CommonUtilities;
using Rage;
using System.Collections.Generic;

namespace GangsOfSouthLS.HelperClasses.ProtectionRacketeeringHelpers
{
    internal enum ConversationState
    {
        Start,
        RefusingToGiveMoney,
        GivingMoneyAfterIntimidation,
        GivingMoneyStraightAway,
        UnintentionallyTellingAboutPolice,
        Attacking,
        Fleeing,
        IntentionallyTellingAboutPolice,
        InformingOfArrival,
        Surprised,
        InformingOfDeparture,
        InformingOfDrivingPast,
        Survived,
        PursuitStillRunning
    }

    internal class ConversationLine
    {
        internal string TalkerColor { get; private set; }
        internal string Talker { get; private set; }
        internal string Line { get; private set; }

        internal int Duration
        {
            get
            {
                return 1900 + (Line.Length * 18);
            }
        }

        internal ConversationLine(string Talker, string Line)
        {
            this.Talker = Talker;
            this.Line = Line;
            if (Talker == "Racketeer")
            {
                TalkerColor = "~r~";
            }
            else if (Talker == "Shopkeeper")
            {
                TalkerColor = "~o~";
            }
            else
            {
                TalkerColor = "";
            }
        }
    }

    internal class ConversationPart
    {
        internal int NumberOfLines { get { return Lines.Count; } }
        internal List<ConversationLine> Lines { get; private set; }

        internal ConversationPart(List<ConversationLine> lines)
        {
            Lines = lines;
        }
    }

    internal class ConversationPartsCollection
    {
        private List<ConversationPart> convPartList;

        internal ConversationState ConvState { get; private set; }
        internal ConversationPart RandomConvPart { get { return convPartList.RandomElement(); } }

        internal ConversationPartsCollection(ConversationState convState, List<ConversationPart> convParts)
        {
            ConvState = convState;
            convPartList = convParts;
        }
    }

    internal class RacketConversation
    {
        private Dictionary<ConversationState, ConversationPartsCollection> convPartCollectionDict;
        private bool finishedTalking;
        private bool breakConversation;

        internal bool IsFinished { get; private set; }

        internal RacketConversation(List<ConversationPartsCollection> ConvPartsCollections)
        {
            convPartCollectionDict = new Dictionary<ConversationState, ConversationPartsCollection> { };
            foreach (var convPartsColl in ConvPartsCollections)
            {
                convPartCollectionDict.Add(convPartsColl.ConvState, convPartsColl);
            }
            finishedTalking = true;
        }

        internal void PlayConversationPartOfState(ConversationState ConvState, bool haltPreviousConversationPart = false)
        {
            IsFinished = false;
            var convPartColl = convPartCollectionDict[ConvState];
            var convPart = convPartColl.RandomConvPart;
            GameFiber.StartNew(delegate
            {
                if (haltPreviousConversationPart)
                {
                    breakConversation = true;
                }
                while (!finishedTalking)
                {
                    GameFiber.Yield();
                }
                finishedTalking = false;
                foreach (var line in convPart.Lines)
                {
                    if (!finishedTalking)
                    {
                        Game.DisplaySubtitle(string.Format("{0}{1}~w~: {2}", line.TalkerColor, line.Talker, line.Line), line.Duration);
                        GameFiber.Wait(line.Duration + 200);
                        if (breakConversation)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                finishedTalking = true;
                IsFinished = true;
            });
        }
    }
}