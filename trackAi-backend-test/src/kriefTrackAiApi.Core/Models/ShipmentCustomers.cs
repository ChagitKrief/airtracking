using System;

namespace kriefTrackAiApi.Core.Models;

  public class ShipmentCustomers
  {
      public Guid Id { get; set; } // Primary Key
      public string ShipmentId { get; set; } = string.Empty;
      public int CustomerNumber { get; set; }
  }