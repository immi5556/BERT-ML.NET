namespace ark.bert.bible
{
    public class BibleManager
    {
        public static List<Book> books = new List<Book>();
        public static Dictionary<string, Dictionary<int, List<Book>>> hie = new Dictionary<string, Dictionary<int, List<Book>>>();
        static BibleManager()
        {
            books = new Ark.Sqlite.SqliteManager(con_str).ExecuteSelect<Book>(select_books_kjv()).ToList();
            books.ForEach(t =>
            {
                if (!hie.ContainsKey(t.book)) hie.Add(t.book, new Dictionary<int, List<Book>>());
                if (!hie[t.book].ContainsKey(t.c)) hie[t.book].Add(t.c, new List<Book>());
                hie[t.book][t.c].Add(t);
            });
        }
        static string con_str = $"Data Source=./Models/bible-sqlite.db";
        //static Func<string> select_books_kjv = () => @$"SELECT b, n as book from key_english";
        static Func<string> select_books_kjv = () => @$"SELECT t.b, ke.n as book, t.c, t.v, t.id, t.t from key_english ke, t_kjv t where t.b = ke.b";
    }
    public class Book
    {
        public int b { get; set; } //book index
        public string book { get; set; }
        public int c { get; set; } //chapter
        public int v { get; set; } //verse
        public string t { get; set; } //text (verse)
        public long id { get; set; }
    }
    public class Question
    {
        public string context { get; set; }
        public string[] question { get; set; }
    }
}
