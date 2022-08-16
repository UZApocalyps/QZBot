using QZBot;

QzBotInstance bot = new QzBotInstance();
bot.start();
AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

void CurrentDomain_ProcessExit(object? sender, EventArgs e)
{
   bot.SaveRooms();
}

while (true)
{

}