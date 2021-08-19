using System.Collections;
using System.IO;
using System.Threading.Tasks;

namespace Energinet.DataHub.PostOffice.Domain
{
    public enum Origin
    {
    }

    public record MessageType(int MaxWeight, string Type);

    public record Weight(int Value);

    public record Recipient(string Value);

    public record Uuid(string Value);

    public class DataAvailableNotification
    {
        public DataAvailableNotification(Uuid id, Recipient recipient, MessageType messageType, Origin origin, Weight weight)
        {
            Id = id;
            Recipient = recipient;
            MessageType = messageType;
            Origin = origin;
            Weight = weight;
        }

        public Uuid Id { get; }
        public Recipient Recipient { get; }
        public MessageType MessageType { get; }
        public Origin Origin { get; }
        public Weight Weight { get; }
    }

    public class Bundle
    {
        public Uuid Id { get; }
        public IEnumerable<Uuid> Notificationsids { get; }
        public Task<Stream> OpenAsync();
    }

    public interface IDataAvailableNotificationRepository
    {
        Task CreateAsync(DataAvailableNotification dataAvailableNotification);
        Task<IEnumerable<DataAvailableNotification>> PeekAsync(Recipient recipient, MessageType messageType);
        Task<DataAvailableNotification?> PeekAsync(Recipient recipient);
        Task DequeueAsync(IEnumerable<Uuid> ids);
    }

    public interface IBundleRepository
    {
        Task<Bundle?> PeekAsync(Recipient recipient);
        Task<Bundle> CreateBundleAsync(IEnumerable<DataAvailableNotification> dataAvailableNotifications);
        Task DequeueAsync(Uuid id);
    }

    public interface IWarehouseDomainService
    {
        Task<Bundle?> PeekAsync(Recipient recipient);
        Task DequeueAsync(Recipient recipient);
    }

    public class WarehouseDomainService : IWarehouseDomainService
    {
        private readonly IBundleRepository _bundleRepository;
        private readonly IDataAvailableNotificationRepository _dataAvailableRepository;

        public WarehouseDomainService(IBundleRepository bundleRepository, IDataAvailableNotificationRepository dataAvailableRepository)
        {
            _bundleRepository = bundleRepository;
            _dataAvailableRepository = dataAvailableRepository;
        }

        public async Task<Bundle?> PeekAsync(Recipient recipient)
        {
            var bundle = await _bundleRepository.PeekAsync(recipient).ConfigureAwait(false);

            if (bundle != null)
                return bundle;

            var dataAvailableNotification = await _dataAvailableRepository.PeekAsync(recipient).ConfigureAwait(false);

            if (dataAvailableNotification != null)
            {
                var dataAvailableNotifications = await _dataAvailableRepository.PeekAsync(recipient, dataAvailableNotification.MessageType).ConfigureAwait(false);
                return await _bundleRepository.CreateBundleAsync(dataAvailableNotifications).ConfigureAwait(false);
            }

            return null;
        }

        public async Task DequeueAsync(Recipient recipient)
        {
            var bundle = await _bundleRepository.PeekAsync(recipient).ConfigureAwait(false);

            if (bundle == null)
                return;

            await _dataAvailableRepository.DequeueAsync(bundle.Notificationsids).ConfigureAwait(false);
            await _bundleRepository.DequeueAsync(bundle.Id).ConfigureAwait(false);
        }
    }
}
