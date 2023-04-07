namespace UKRTB_journal.Models
{
	public class EmailSettings
	{
		public string Host { get; set; }

		public int Port { get; set; }

		public string Username { get; set; }

		public string Password { get; set; }

		public string From { get; set; }

		public string Sender { get; set; }

		public string Email { get; set; }

		public bool SmtpAutoSecureSocketOptions { get; set; }
	}

	public class EmailMessage
	{
		public string From { get; set; }

		public IEnumerable<string> To { get; set; }

		public string Subject { get; set; }

		public string Body { get; set; }

		public bool Importance { get; set; }
	}
}
