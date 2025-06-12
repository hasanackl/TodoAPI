using Microsoft.AspNetCore.Mvc;

namespace Example02.Controllers
{
    [Route("Products")]
    public class ProductsController : ControllerBase
    {
        [Route("Merhaba")]
        public string Merhaba()
        {
            return "Merhaba";
        }
        [Route("Hello")]
        public string Hello()
        {
            return "Hello";
        }
    }
}
