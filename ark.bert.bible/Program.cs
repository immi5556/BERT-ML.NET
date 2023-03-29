using ark.bert.bible;
using Microsoft.ML.Models.BERT;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<BibleManager>();
builder.Services.AddSingleton(o =>
{
    var modelConfig = new BertModelConfiguration()
    {
        VocabularyFile = "./Models/vocab.txt",
        ModelPath = "./Models/bertsquad-10.onnx"
    };

    var model = new BertModel(modelConfig);
    model.Initialize();

    return model;
});
//builder.Services.AddScoped<>
// Add services to the container.
builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
