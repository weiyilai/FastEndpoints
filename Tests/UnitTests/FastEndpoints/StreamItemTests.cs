using FastEndpoints;
using Xunit;

namespace StreamItemTests;

public class StreamItemTests
{
    [Fact]
    public void id_can_be_set_after_construction()
    {
        var item = new StreamItem("my-event", "payload");

        item.Id = "42";

        item.Id.ShouldBe("42");
        item.EventName.ShouldBe("my-event");
        item.Data.ShouldBe("payload");
    }
}
