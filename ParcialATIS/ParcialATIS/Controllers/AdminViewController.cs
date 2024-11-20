using Microsoft.AspNetCore.Mvc;

public class AdminViewController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
