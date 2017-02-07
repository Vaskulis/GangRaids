using GangsOfSouthLS.HelperClasses.ProtectionRacketeeringHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangsOfSouthLS.Scenarios.ProtectionRacketeeringScenarios
{
    static class RacketConversationScheme
    {
        private static ConversationPartsCollection StartConversationParts = new ConversationPartsCollection(ConversationState.Start, new List<ConversationPart>
        {
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Racketeer", "Hello there, it's so nice to see you again."),
                new ConversationLine("Racketeer", "I believe you have something for me, don't you."),
                new ConversationLine("Racketeer", "Or have you forgotten what happened the last time you didn't?"),
                new ConversationLine("Racketeer", "I hope I don't need to remind you.")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Racketeer", "Hello again, I think you know what I'm here for."),
                new ConversationLine("Racketeer", "So give me the money and I'll be on my way."),
                new ConversationLine("Racketeer", "You wouldn't want all those gangsters around here smashing up your shop, would you?"),
                new ConversationLine("Racketeer", "I'm only here for what we deserve for protecting you.")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Racketeer", "Good day, I hope everything is all right."),
                new ConversationLine("Racketeer", "You never know in this neighbourhood."),
                new ConversationLine("Racketeer", "Good thing you can always count on us for protecting you."),
                new ConversationLine("Racketeer", "So just hand over our fair share and you won't have any problems."),
                new ConversationLine("Racketeer", "From us or anyone else.")
            })
        });

        private static ConversationPartsCollection RefusingToGiveMoneyConversationParts = new ConversationPartsCollection(ConversationState.RefusingToGiveMoney, new List<ConversationPart>
        {
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "I'm sorry, but I can't give you anything today."),
                new ConversationLine("Shopkeeper", "Sales are down and if I give you the money,"),
                new ConversationLine("Shopkeeper", "I won't have anything left for my family."),
                new ConversationLine("Racketeer", "Well, maybe we should pay your family a visit then."),
                new ConversationLine("Racketeer", "Explain to them the importance of getting your priorities straight.")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "Yeah, well I don't have anything right now."),
                new ConversationLine("Shopkeeper", "You will have to come back in a few days."),
                new ConversationLine("Racketeer", "A lot can happen in a few days."),
                new ConversationLine("Racketeer", "It would be a shame if something happened to your shop in the meantime."),
                new ConversationLine("Racketeer", "We wouldn't want that, would we?")
            }),
            new ConversationPart(new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "Screw you! I have given you money for the last time!"),
                new ConversationLine("Shopkeeper", "Do you think you can come here every week and just take all my money?"),
                new ConversationLine("Racketeer", "Shut up and think about how you are talking to me!"),
                new ConversationLine("Racketeer", "You know exactly what will happen if I don't get what I'm here for."),
                new ConversationLine("Racketeer", "And this time, you won't get away with a black eye!")
            })
        });

        private static ConversationPartsCollection GivingMoneyAfterIntimidationConversationParts = new ConversationPartsCollection(ConversationState.GivingMoneyAfterIntimidation, new List<ConversationPart>
        {
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "Calm down! There's no need for that."),
                new ConversationLine("Shopkeeper", "I'll give you the money."),
                new ConversationLine("Racketeer", "See, this wasn't so hard!"),
                new ConversationLine("Racketeer", "I hope next time you'll be in a better mood.")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "All right, all right, you'll get what you want."),
                new ConversationLine("Shopkeeper", "Just don't hurt me again."),
                new ConversationLine("Racketeer", "I knew we would understand each other."),
                new ConversationLine("Racketeer", "See you next time.")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "Okay, just relax, here is the money."),
                new ConversationLine("Shopkeeper", "There is no need for this to end violently."),
                new ConversationLine("Racketeer", "I'm glad we could agree on that."),
                new ConversationLine("Racketeer", "Let's hope that next time I won't have to remind you of it.")
            })
        });

        private static ConversationPartsCollection GivingMoneyStraightAwayConversationParts = new ConversationPartsCollection(ConversationState.GivingMoneyStraightAway, new List<ConversationPart>
        {
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "Yeah, just take everything I own!"),
                new ConversationLine("Racketeer", "You know it's for your own good."),
                new ConversationLine("Shopkeeper", "So you keep telling me. Here you go."),
                new ConversationLine("Racketeer", "You won't regret it. See you next time!")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "All right, all right, you'll get what you want."),
                new ConversationLine("Shopkeeper", "Just don't hurt me again."),
                new ConversationLine("Racketeer", "I knew we would understand each other."),
                new ConversationLine("Racketeer", "I'll be back next week.")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "I know the drill, no need to give me the whole speech."),
                new ConversationLine("Shopkeeper", "Here is the money."),
                new ConversationLine("Racketeer", "I'm glad we understand each other. Until next time!")
            })
        });

        private static ConversationPartsCollection UnintentionallyTellingAboutPoliceConversationParts = new ConversationPartsCollection(ConversationState.UnintentionallyTellingAboutPolice, new List<ConversationPart>
        {
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "Yeah... sure, next time."),
                new ConversationLine("Racketeer", "What's wrong?"),
                new ConversationLine("Shopkeeper", "Oh, nothing. Nothing at all!"),
                new ConversationLine("Racketeer", "You fucking asshole called the cops on us!")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "I wouldn't be so sure about that."),
                new ConversationLine("Racketeer", "What the hell are you talking about?"),
                new ConversationLine("Shopkeeper", "They've heard everything!"),
                new ConversationLine("Shopkeeper", "You'll be rotting in a cell for the next few years!")
            }),
            new ConversationPart(new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "Yeah, if they don't lock you up in the meantime."),
                new ConversationLine("Shopkeeper", "I mean, you know... hypothetically."),
                new ConversationLine("Racketeer", "Wow, you really are as dumb as you look."),
                new ConversationLine("Racketeer", "You really shouldn't have done this!")
            })
        });

        private static ConversationPartsCollection AttackingConversationParts = new ConversationPartsCollection(ConversationState.Attacking, new List<ConversationPart>
        {
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Racketeer", "That was your last mistake!"),
                new ConversationLine("Shopkeeper", "Oh God! Help me!")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Racketeer", "I will fuck you up!"),
                new ConversationLine("Shopkeeper", "Oh no, please no!")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Racketeer", "You won't live to regret this!"),
                new ConversationLine("Shopkeeper", "Come now! Now, I need help!")
            })
        });

        private static ConversationPartsCollection FleeingConversationParts = new ConversationPartsCollection(ConversationState.Fleeing, new List<ConversationPart>
        {
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Racketeer", "Fuck you! They won't get us this time!"),
                new ConversationLine("Shopkeeper", "They're getting away! Go after them!")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Racketeer", "You're going to regret this! We'll be back!"),
                new ConversationLine("Shopkeeper", "Don't let them get away! Please!")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Racketeer", "They won't get us! Not if I can help it!"),
                new ConversationLine("Shopkeeper", "They are making a run for it! Arrest them already!")
            })
        });

        private static ConversationPartsCollection IntentionallyTellingAboutPoliceConversationParts = new ConversationPartsCollection(ConversationState.IntentionallyTellingAboutPolice, new List<ConversationPart>
        {
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "You think you can threaten me?"),
                new ConversationLine("Shopkeeper", "The cops are listening and I bet they don't like what they're hearing!"),
                new ConversationLine("Racketeer", "I can't believe you would be that stupid!")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "I... I can't do this any more!"),
                new ConversationLine("Shopkeeper", "Help me! Get me out of here!"),
                new ConversationLine("Racketeer", "You're bugged! What the hell?")
            }),

        });

        private static ConversationPartsCollection InformingOfArrivalConversationParts = new ConversationPartsCollection(ConversationState.InformingOfArrival, new List<ConversationPart>
        {
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "They're coming! God I hope you can hear me!")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "Here they come. You better be ready!")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "Can you hear me? Here they are.")
            })
        });

        private static ConversationPartsCollection SurprisedConversationParts = new ConversationPartsCollection(ConversationState.Surprised, new List<ConversationPart>
        {
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Racketeer", "What the fuck?")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Racketeer", "What the hell?")
            })
        });

        private static ConversationPartsCollection InformingOfDrivingPastConversationParts = new ConversationPartsCollection(ConversationState.InformingOfDrivingPast, new List<ConversationPart>
        {
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "That's them, but... they're driving on!"),
                new ConversationLine("Shopkeeper", "They must've seen you! My life is over!")
            }),
            new ConversationPart( new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "Here they come! Wait... Where are they going?"),
                new ConversationLine("Shopkeeper", "My god, you have let them see you!"),
                new ConversationLine("Shopkeeper", "They're going to kill me!")
            })
        });

        private static ConversationPartsCollection InformingOfDepartureConversationParts = new ConversationPartsCollection(ConversationState.InformingOfDeparture, new List<ConversationPart>
        {
            new ConversationPart(new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "That's it, they're gone. Now arrest them!")
            }),
            new ConversationPart(new List<ConversationLine>
            {
                new ConversationLine("Shopkeeper", "Alright, they're driving away."),
                new ConversationLine("Shopkeeper", "Now would be a good time to pull over their car and arrest them!")
            }),
        });

        internal static List<ConversationPartsCollection> ConverstaionPartsCollections = new List<ConversationPartsCollection>
        {
            StartConversationParts,
            AttackingConversationParts,
            FleeingConversationParts,
            GivingMoneyAfterIntimidationConversationParts,
            RefusingToGiveMoneyConversationParts,
            GivingMoneyStraightAwayConversationParts,
            UnintentionallyTellingAboutPoliceConversationParts,
            IntentionallyTellingAboutPoliceConversationParts,
            InformingOfArrivalConversationParts,
            SurprisedConversationParts,
            InformingOfDrivingPastConversationParts,
            InformingOfDepartureConversationParts
        };
    }
}
