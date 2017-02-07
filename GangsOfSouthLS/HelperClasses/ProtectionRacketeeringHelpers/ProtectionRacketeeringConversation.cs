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
        InformingOfDrivingPast
    }

    internal class ConversationLine
    {
        private string talker;
        private string line;
        private string talkercolor;

        internal ConversationLine(string Talker, string Line)
        {
            talker = Talker;
            line = Line;
            if (Talker == "Racketeer") { talkercolor = "~r~"; }
            else if (Talker == "Shopkeeper") { talkercolor = "~o~"; }
            else { talkercolor = ""; }
        }

        internal string TalkerColor { get { return talkercolor; } }
        internal string Talker { get { return talker; } }
        internal string Line { get { return line; } }

        internal int Duration
        {
            get
            {
                return 1700 + (line.Length * 15);
            }
        }
    }

    internal class ConversationPart
    {
        private List<ConversationLine> lineList;

        internal ConversationPart(List<ConversationLine> Lines)
        {
            lineList = Lines;
        }

        internal int NumberOfLines { get { return lineList.Count; } }
        internal List<ConversationLine> Lines { get { return lineList; } }
    }

    internal class ConversationPartsCollection
    {
        private ConversationState convState;
        private List<ConversationPart> convPartList;

        internal ConversationPartsCollection(ConversationState ConvState, List<ConversationPart> ConvParts)
        {
            convState = ConvState;
            convPartList = ConvParts;
        }

        internal ConversationState ConvState { get { return convState; } }
        internal ConversationPart RandomConvPart { get { return convPartList.RandomElement(); } }
    }

    internal class Conversation
    {
        protected Dictionary<ConversationState, ConversationPartsCollection> convPartCollectionDict;
        protected bool finishedTalking;
        protected bool finishedState;
        protected bool breakConversation;

        internal bool IsFinished { get { return finishedState; } }

        internal Conversation(List<ConversationPartsCollection> ConvPartsCollections)
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
            finishedState = false;
            var convPartColl = convPartCollectionDict[ConvState];
            var convPart = convPartColl.RandomConvPart;
            GameFiber.StartNew(delegate
            {
                if (haltPreviousConversationPart)
                {
                    breakConversation = true;
                    while (!finishedTalking)
                    {
                        GameFiber.Yield();
                    }
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
                        if (breakConversation) { break; }
                    }
                    else
                    {
                        break;
                    }
                }
                finishedTalking = true;
                finishedState = true;
            });
        }
    }
}