using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator
{
    /// <summary>
    /// bla
    /// </summary>
    public interface IRequestBundleRequestParser
    {
        /// <summary>
        /// bla
        /// </summary>
        /// <param name="dataBundleReplyContract"></param>
        /// <returns>1</returns>
        RequestDataBundleResponseDto Parse(byte[] dataBundleReplyContract);
    }
}
