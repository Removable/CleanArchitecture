using System.Net;

namespace CleanArchitecture.Web.EndpointGroups;

public sealed class TodoListGroup : Group
{
    public TodoListGroup()
    {
        Configure("TodoLists",
            ep =>
            {
                ep.Description(x => x
                    .Produces((int)HttpStatusCode.Unauthorized)
                    .WithTags("TodoLists"));
            });
    }
}
