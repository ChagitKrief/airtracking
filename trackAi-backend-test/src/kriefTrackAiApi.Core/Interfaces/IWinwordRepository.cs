using System.Threading.Tasks;
using kriefTrackAiApi.Common.Dto;

namespace kriefTrackAiApi.Core.Interfaces;

  public interface IWinwordRepository
  {
      Task<List<DataItem>> FetchShipmentDataAsync(string token);
      Task FetchAndSaveSmsDataAsync();
  }
