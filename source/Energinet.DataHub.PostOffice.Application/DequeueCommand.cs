namespace Energinet.DataHub.PostOffice.Application
{
    public class DequeueCommand
    {
        /// <summary>
        /// Body for document store.
        /// </summary>
        public DequeueCommand(string recipient, string bundle)
        {
            Recipient = recipient;
            Bundle = bundle;
        }

        /// <summary>
        /// Recipient.
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// Bundle.
        /// </summary>
        public string Bundle { get; set; }
    }
}
