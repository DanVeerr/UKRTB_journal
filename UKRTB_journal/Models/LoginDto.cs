using System.ComponentModel.DataAnnotations;

namespace UKRTB_journal.Models
{
    /// <summary>
    /// Дто авторизации
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Email
        /// </summary>
        [Required(ErrorMessage = "Логин обязателен")]
        [EmailAddress(ErrorMessage = "Неправильный формат для email адреса")]
        public string Login { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
