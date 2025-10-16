using System.Net;

namespace CleanArchitecture.Web.EndpointGroups;

public sealed class TodoItemGroup : Group
{
    public TodoItemGroup()
    {
        Configure("TodoItems",
            ep =>
            {
                ep.Description(x => x
                    .Produces((int)HttpStatusCode.Unauthorized)
                    .WithTags("TodoItems"));
            });
    }
}
