using Microsoft.AspNetCore.Mvc;

namespace Shared.Network
{
    public sealed class JsonContentResult : ContentResult
    {
        public JsonContentResult()
        {
            ContentType = "application/json";
        }
    }
}
