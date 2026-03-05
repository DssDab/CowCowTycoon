using CowCowTycoon.Datas.AuctionData;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static CowCowTycoon.Utils.ConvertDataType;
public class NetworkUtility 
{
    private NetworkConfig _cfg;

    public NetworkUtility(NetworkConfig cfg)
    {
        _cfg = cfg;
    }
   
    public async Task<int> GetTodayAuctAmtAsync(CancellationToken ct, int timeOutSec = 10)
    {
        DateTimeOffset kst = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(9));
        string url = _cfg != null ? _cfg.serverUrl : null;
        string serviceKey = ResolveServiceKey(_cfg);
        if(string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("SERVER_URL_EMPTY");
            return 0;
        }
        if(string.IsNullOrEmpty(serviceKey))
        {
            Debug.LogWarning("SERVICE_KEY_EMPTY");
            return 0;
        }
        url += "?serviceKey=" + UnityWebRequest.EscapeURL(serviceKey);
        url += $"&baseYmd={kst.ToString("yyyy-MM-dd")}";

        using var req = UnityWebRequest.Get(url);
        req.timeout = timeOutSec;

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeOutSec));

        using var abortReg = timeoutCts.Token.Register(() =>
        {
            try { req.Abort(); } catch { }
        });
        try
        {
            var op = req.SendWebRequest();

            while(op.isDone == false)
            {
                timeoutCts.Token.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            if (req.result != UnityWebRequest.Result.Success)
                throw new Exception($"HTTP_FAIL code={req.responseCode}, error={req.error})");
        }
        catch (OperationCanceledException)
        {
            bool isTimeout = timeoutCts.IsCancellationRequested && !ct.IsCancellationRequested;
            if (isTimeout)
                throw new TimeoutException($"TIMEOUT after {timeOutSec}s");

            throw;
        }


        string json;
        try
        {
            json = ConvertXmlToJson(req.downloadHandler.text);
        }
        catch(Exception ex)
        {
            throw new Exception($"XML_TO_JSON_FAIL : {ex.Message}");
        }
        Response response;
        try
        {
            response = JsonUtility.FromJson<Response>(json);
        }
        catch(Exception ex) 
        {
            throw new Exception($"JSON_PARSE_FAIL : {ex.Message}");
        }

        int idx = _cfg != null ? _cfg.nationalIndex : 2;
        if (response == null || response.body == null || response.body.items == null || response.body.items.Count <= idx)
        {
            int len = response?.body?.items?.Count ?? -1;
            throw new Exception($"SHAPE_FAIL : itemLength={len}, requiredIndex={idx}");
        }

        int amt = response.body.items[idx].auctAmt;
        if (amt <= 0)
            throw new Exception($"INVALID_PRICE : Amt={amt}");

        return amt;
    }

    private string ResolveServiceKey(NetworkConfig cfg)
    {
        if(cfg != null && string.IsNullOrEmpty(cfg.serviceKey) == false)
            return cfg.serviceKey;

        // 빌드 / 로컬에서 주입
        return Environment.GetEnvironmentVariable("COWCOW_EKAPE_KEY") ?? "";
    }
}
