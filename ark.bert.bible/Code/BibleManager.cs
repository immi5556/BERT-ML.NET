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
    public class RecordManger
    {
        public static dynamic GetLogs(string tbl)
        {
            return new Ark.Sqlite.SqliteManager(con_str).Select($"select * from {tbl}");
        }
        public static List<string> GetMonthlyTable()
        {
            string qry = @"SELECT 
                            name
                        FROM 
                            sqlite_schema
                        WHERE 
                            type ='table' AND 
                            name NOT LIKE 'sqlite_%';";
            return new Ark.Sqlite.SqliteManager(con_str).ExecuteSelect<string>(qry).ToList();
        }
        static Func<string> log_table_name = () => $"logs_{DateTime.Now.Year}_{DateTime.Now.Month.ToString().PadLeft(2, '0')}"; // Yeear & MOnth level file
        public static void Log(Question q)
        {
            var tbl = log_table_name();
            new Ark.Sqlite.SqliteManager(con_str).CreateTable(create_table_logs(tbl));
            new Ark.Sqlite.SqliteManager(con_str).ExecuteQuery(insert_log_record(tbl, q));

        }
        static string con_str = $"Data Source=./Models/log-sqlite.db";
        static Func<string?, string> cleanup = str => Ark.Sqlite.SqliteManager.ReplaceSpecialChar(str ?? "", new Dictionary<string, string?>() { { "'", "''" } });
        static Func<string, string> create_table_logs = (tbl_name) =>
        @$"CREATE TABLE  IF NOT EXISTS {tbl_name} (
    log_id INTEGER NOT NULL
                      CONSTRAINT PK_logs_id PRIMARY KEY AUTOINCREMENT,
    books     TEXT,
    context   TEXT,
    ids       TEXT,
    verses    TEXT,
    chapters  TEXT,
    question  TEXT,
    answers   TEXT,
    confidence   TEXT,
    ip        TEXT,
    at        TEXT
);
";
        static Func<string, Question, string> insert_log_record = (tbl_name, question) =>
        @$"INSERT INTO {tbl_name} (
                        books,
                        context,
                        ids,
                        verses,
                        chapters,
                        question,
                        answers,
                        confidence,
                        ip,
                        at
                    )
                    VALUES (
                        '{cleanup(Newtonsoft.Json.JsonConvert.SerializeObject(question.books))}',
                        '{cleanup(question.context)}',
                        '{cleanup(Newtonsoft.Json.JsonConvert.SerializeObject(question.ids))}',
                        '{cleanup(Newtonsoft.Json.JsonConvert.SerializeObject(question.verses))}',
                        '{cleanup(Newtonsoft.Json.JsonConvert.SerializeObject(question.chapters))}',
                        '{cleanup(Newtonsoft.Json.JsonConvert.SerializeObject(question.question))}',
                        '{cleanup(Newtonsoft.Json.JsonConvert.SerializeObject(question.answers))}',
                        '{cleanup(Newtonsoft.Json.JsonConvert.SerializeObject(question.confidence))}',
                        '{question.ip}',
                        '{DateTime.UtcNow}'
                    );
";
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
        public long[] ids { get; set; }
        public int[] verses { get; set; }
        public int[] chapters { get; set; }
        public string[] books { get; set; }
        public string[] question { get; set; }
        public string[] answers { get; set; }
        public float[] confidence { get; set; }
        public string ip { get; set; }
        public string at { get; set; }
    }
}
