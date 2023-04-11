using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Dto
{
    public sealed class JsonContentResult : ContentResult
    {
        public JsonContentResult()
        {
            ContentType = "application/json";
        }
    }
}