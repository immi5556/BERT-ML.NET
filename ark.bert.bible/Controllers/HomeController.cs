using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.ML.Models.BERT;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;

namespace ark.bert.bible.Controllers
{
    public class HomeController : Controller
    {
        private readonly BertModel _bertModel;
        private readonly BibleManager _bible;
        public HomeController(BibleManager bible, BertModel bertModel)
        {
            _bible = bible;
            _bertModel = bertModel;
        }
        public IActionResult TrackLog()
        {
            var tbls = RecordManger.GetMonthlyTable().OrderByDescending(t => t).ToList();
            ViewBag.Tracks = tbls;
            var tbb = tbls.Count > 0 ? tbls[0] : "";
            ViewBag.Tbl = tbb;
            ViewBag.TrackLog = RecordManger.GetLogs(tbb);
            return View();
        }
        [HttpPost]
        public IActionResult TrackLog([FromForm] string table)
        {
            var tbls = RecordManger.GetMonthlyTable().OrderByDescending(t => t).ToList();
            ViewBag.Tracks = tbls;
            var tbb = !string.IsNullOrEmpty(table) ? table : tbls.Count > 0 ? tbls[0] : "";
            ViewBag.Tbl = tbb;
            ViewBag.TrackLog = RecordManger.GetLogs(tbb); 
            return View();
        }
        public IActionResult Index()
        {
            ViewBag.Bible = BibleManager.hie;
            ViewBag.book = "Psalms";
            ViewBag.chapter = 23;
            return View();
        }

        [HttpPost]
        public IActionResult Index([FromForm] string book, [FromForm] int chapter)
        {
            ViewBag.Bible = BibleManager.hie;
            ViewBag.book = string.IsNullOrEmpty(book) ? "Psalms" : book;
            ViewBag.chapter = chapter == 0 ? 1 : BibleManager.hie[book].ContainsKey(chapter) ? chapter : 1;
            return View();
        }

        [HttpPost]
        [Route("predict")]
        public dynamic SubmitQuestion([FromBody] Question question)
        {
            var prob = new List<(string, (List<string>, float))>();
            (question.question ?? new string[] { }).ToList().ForEach(x =>
            {
                prob.Add((x, _bertModel.Predict(question.context, x)));
            });
            var ans = prob.Select(t => "Question: " + t.Item1 + "<br />" + "Answer:" + string.Join(' ', t.Item2.Item1) + "<br />" + "Confidence Level:" + t.Item2.Item2.ToString()).ToList();
            question.answers = prob.Select(t => string.Join(' ', t.Item2.Item1)).ToArray();
            question.confidence = prob.Select(t => t.Item2.Item2).ToArray();
            RecordManger.Log(question);
            return new
            {
                Results = ans
            };
        }
        public dynamic GetBible([FromQuery] string book, [FromQuery] int chapter)
        {
            return BibleManager.hie.ContainsKey(book) && BibleManager.hie[book].ContainsKey(chapter) ? BibleManager.hie[book][chapter] : new { message = "Invalid Chapter or Book passed" };
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}