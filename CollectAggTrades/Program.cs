using Binance.Net;
using Binance.Net.Objects.Spot;
using Binance.Net.SocketSubClients;
using CryptoExchange.Net.Authentication;
using System;
using CryptoExchange.Net.Logging;
using Binance.Net.Objects.Brokerage.SubAccountData;

namespace CollectAggTrades
{
  class Program
  {

    static void AggregatedTrade()
    {
      using (var client = new BinanceSocketClient())
      {

        var successTrades = client.Spot.SubscribeToAggregatedTradeUpdatesAsync("ethusdt", (data) =>
        {
          Console.WriteLine(data.Price);
        });
      }

    }

    static void GetStreamDepth()
    {
      using (var wsclient = new BinanceSocketClient())
      {
        Console.WriteLine("Subscribe to future coin Depth Data");
        var successTrades = wsclient.FuturesCoin.SubscribeToOrderBookUpdatesAsync("ethusd_perp", 500,
          (onMessage) =>
          {
            foreach (var ask in onMessage.Asks)
            {
              Console.WriteLine($"{ask.Quantity} : Amount={ask.Price} ");
            }
          });
      }

    }

    static void GetAllFutureCoinSymbols(BinanceClient client)
    {
      var allPrice = client.FuturesCoin.Market.GetAllPrices();

      foreach (var data in allPrice.Data)
      {
        Console.WriteLine($" {data.Symbol} : {data.Price} ");
      }
    }

    static void Main(string[] args)
    {
      var client = new BinanceClient(new BinanceClientOptions
      {
        ApiCredentials = new ApiCredentials("qzQApE9sTemaP6Qda3eGW4n3HLOfp1LaQ6Q4ZFS1kA7qTSOCVJeISD7G4JBQsR4b", "w0hkzxOyyB4ryGjR2aiAQtgqqBbydG8RQ2Zs5fNJCcJCH4rtiW5zs8fs7EQ7Yh6r")
      });


      var generalInfo = client.General.GetAccountInfo().Data;
#if USE_SPOT_ACCOUNT
      Console.WriteLine("===== Spot account =========");
      foreach (var bal in generalInfo.Balances)
      {
        if (bal.Free > (decimal)0.0)
        {
          Console.WriteLine(string.Format("{0} - {1}", bal.Asset, bal.Free));
        }
      }
#endif
      //	var accStatus = client.General.GetAccountStatus().Data;
      
      Console.WriteLine("===== Future account =========");
#if USE_FUTURE_ACCOUNT
      var futureAccountcSnapshot = client.General.   GetDailyFutureAccountSnapshot().Data;
      foreach (var sn in futureAccountcSnapshot)
      {
        foreach (var asset in sn.Data.Assets)
        {
          Console.WriteLine(string.Format("{0} {1}  Margin: {2} Wallet: {3}", sn.Timestamp.ToString(), asset.Asset, asset.MarginBalance, asset.WalletBalance));

        }
      }
#endif
//#if USE_FUTURE_COIN_SYMB
      GetAllFutureCoinSymbols(client);
//#endif


      ///  var log = new Log("test");
      //  var opt = new BinanceSocketClientOptions();
      //  //var wsclientfc = new BinanceSocketClient();
      // wsclientfc.FuturesCoin.SubscribeToOrderBookUpdatesAsync()
      var startResult = client.Spot.UserStream.StartUserStream();

      //      if (!startResult.Success)
      //        throw new Exception($"Failed to start spot user stream: {startResult.Error}");


//      startResult = client.FuturesCoin.UserStream.StartUserStream();
//if (!startResult.Success)
//      throw new Exception($"Failed to start FuturesCoin user stream: {startResult.Error}");

#if USE_ORDERBOOK
      using (var wsclient = new BinanceSocketClient())
      {
        var successTrades = wsclient.FuturesCoin.SubscribeToOrderBookUpdatesAsync("ethusd_perp", 500, (msg) => {
        Console.WriteLine($" {msg.Symbol}");
          foreach(var ask in msg.Asks)
            Console.WriteLine( $"ask:{ask.Price} {ask.Quantity}");
          foreach(var bid in msg.Bids)
            Console.WriteLine($"bid:{bid.Price} {bid.Quantity}");

        });
      }
#endif

#if USE_AGGTRADE
      using (var wsclient = new BinanceSocketClient())
      {

        var successTrades = wsclient.FuturesCoin.SubscribeToAggregatedTradeUpdatesAsync("adausd_perp",  (msg) => {
          Console.WriteLine($" {msg.Symbol} price:{msg.Price} Q:{msg.Quantity} BuyerMaker:{msg.BuyerIsMaker}");
        });
      }
#endif

      // future transactions
      using (var wsclient = new BinanceSocketClient())
      {
        Console.WriteLine("Subscribe to User Data");
        var successTrades = wsclient.FuturesCoin.SubscribeToUserDataUpdatesAsync("adause_perp",
        (marginData) =>
        {
          foreach (var pos in marginData.Positions)
          {
            Console.WriteLine($"{pos.Symbol} : Amount={pos.PositionAmount} ");
          }
        },
        (accData) =>
        {
          var updateData = accData.UpdateData;
          foreach (var pos in accData.UpdateData.Positions)
          {
            Console.WriteLine($"{pos.Symbol} Entry:{pos.EntryPrice} Amount:{ pos.PositionAmount } PnL:{pos.RealizedPnL} Side:{pos.PositionSide} MarginType:{pos.MarginType} {pos.EntryPrice} ");
          }
        },
        (orderData) =>
        {
          var data = orderData.UpdateData;
          Console.WriteLine($"{data.Symbol} Quantity:{data.Quantity} Price:{data.Price} PosSide:{data.PositionSide} Side:{data.Side} Commision:{data.Commission} CommisionAsset:{data.CommissionAsset} Avg Price:{data.AveragePrice} BuyerIsMaker:{data.BuyerIsMaker} ");
        },
        (keyEvent) =>
        {
          Console.WriteLine($"{keyEvent.EventTime} {keyEvent.Event}");
        });

        if (client.FuturesUsdt.UserStream.StartUserStream().Success)
        {
          Console.WriteLine("UserStream Start");
        }
      }


      //var socketClient = new BinanceSocketClient();
      //// subscribe to updates on the spot API
      //	socketClient.Spot.SubscribeToBookTickerUpdates("BTCUSDT", data => {
      //		Console.WriteLine(data.BestBidQuantity);
      //	// Handle data
      //});



      Console.Write("\nPress Any Key To Stop Streaming");
        Console.ReadKey();
        client.FuturesUsdt.UserStream.StopUserStream("ethusdt");
        Console.Write("\nPress Any Key To Exit");
        Console.ReadKey();
        //var startResult = client.Spot.UserStream.StartUserStream();

        //if (!startResult.Success)
        //	throw new Exception($"Failed to start user stream: {startResult.Error}");

        //var socketClient = new BinanceSocketClient();

        //		socketClient.Spot.SubscribeToTradeUpdatesAsync(startResult.Data,
        //			accountUpdate => { // Handle account info update 
        //},
        //			orderUpdate => { // Handle order update
        //},
        //			ocoUpdate => { // Handle oco order update
        //},
        //			positionUpdate => { // Handle account position update
        //},
        //			balanceUpdate => { // Handle balance update
        //});
      }
    }
 // }
}