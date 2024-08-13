namespace AIDemo.Web.Messages;

public record ContactFactResult(string ContactName, string Fact);

public record SearchContactsResult(IReadOnlyList<string> Contacts);

public record ContactBirthdayResult(string ContactName, ContactBirthday Birthday);
