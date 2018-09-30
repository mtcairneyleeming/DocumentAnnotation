using Microsoft.AspNetCore.Mvc;

namespace DocumentAnnotation.Controllers
{
    public class ReloadController : Controller
    {
        private readonly TextLoader.TextLoader _textLoader;


        public ReloadController(TextLoader.TextLoader textLoader)
        {
            _textLoader = textLoader;
        }

        [HttpPost]
        public EmptyResult Reload()
        {
            _textLoader.ForceReload();
            return new EmptyResult();
        }
    }
}