namespace ForumParser.DbLibrary
{
    public class ForumMessage
    {
        public long Id { get; set; }        // Уникальное целое число (ID сообщения с форума)
        public string Name { get; set; }   // Логин (до 256 символов)
        public string Message { get; set; } // Текст сообщения (до 8096 символов)
    }
}
