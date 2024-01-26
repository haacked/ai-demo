using Haack.AIDemoWeb.Entities;

namespace Serious.ChatFunctions;

/// <summary>
/// Extends Chat GPT with a function that stores facts about a user.
/// </summary>
public class StoreUserRelationshipFunction : StoreUserFactFunction
{
    readonly AIDemoContext _db;
    readonly OpenAIClientAccessor _client;

    public StoreUserRelationshipFunction(AIDemoContext db, OpenAIClientAccessor client) : base(db, client)
    {
        _db = db;
        _client = client;
    }

    protected override string Name => "store_user_relationship";

    protected override string Description => "Stores information about a user's relationship when the user makes a declarative statement about another person. For example, with the statement \"My son is @tyrion\" the username is the current user and the fact is that the user's son is tyrion.";
}
