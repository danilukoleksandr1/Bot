using Bot;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<GardenAssistantBot>(); // Додали бота до колекції служб

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var bot = app.Services.GetRequiredService<GardenAssistantBot>(); // Отримуємо екземпляр бота з контейнера служб
bot.Start(); // Запуск бота

app.Run();
