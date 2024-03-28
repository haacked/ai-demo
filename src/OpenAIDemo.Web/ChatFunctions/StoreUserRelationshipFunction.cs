using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;

namespace Serious.ChatFunctions;

/// <summary>
/// Extends Chat GPT with a function that stores facts about a user.
/// </summary>
public class StoreUserRelationshipFunction(AIDemoContext db, OpenAIClientAccessor client, GeocodeClient geocodeClient)
    : StoreUserFactFunction(db, client, geocodeClient)
{
    protected override string Name => "store_user_relationship";

    protected override string Description => "Stores information about a user's relationship when the user makes a declarative statement about another person. For example, with the statement \"My son is @tyrion\" the username is the current user and the fact is that the user's son is tyrion.";
}
