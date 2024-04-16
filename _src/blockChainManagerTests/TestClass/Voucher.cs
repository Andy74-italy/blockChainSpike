using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace blockChainManagerTests.TestClass
{
    /// <summary>
    /// The basic voucher information that is normally provided during creation.
    /// </summary>
    public class Voucher
    {
        /// <summary>
        ///  Identifies the unique voucher code.
        /// </summary>
        public string voucherCode { get; set; }
        /// <summary>
        /// Extended description of the voucher.
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// Monetary value of the voucher, available to the beneficiary.
        /// </summary>
        public decimal amount { get; set; }
        /// <summary>
        /// Voucher issue date.
        /// </summary>
        public DateTime issuanceDate { get; set; }
        /// <summary>
        /// Date by which the voucher must be used.
        /// </summary>
        public DateTime expiryDate { get; set; }
        /// <summary>
        /// Voucher holder who requested it. The vouchers, normally nominal, 
        /// could be used by someone who is not the owner (beneficiary).
        /// </summary>
        public Actor holder { get; set; }
    }

    /// <summary>
    /// Basic personal data of a user (whether owner or beneficiary).
    /// </summary>
    public class Actor
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string fullName => $"{firstName} {lastName}";
        public string fiscalCode { get; set; }
        public string email { get; set; }
    }
}
