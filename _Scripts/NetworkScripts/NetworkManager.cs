using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class NetworkManager {

    public const float NETWORK_ANIMATOR_UPDATE_PERIOD_SECONDS = 0.1f;
    public const float NETWORK_UPDATE_PERIOD_SECONDS = 1f;
    public const float LOBBY_UPDATE_PERIOD_SECONDS = 3f;

    private GuestNetwork g;
    private HostNetwork h;

    private string username;      

    public void regularUpdate() {
        if (h != null) {
            h.regularUpdate();
        } else if (g != null) {
            g.regularUpdate();
            if (g.isBecomeHost()) {
                h = new HostNetwork(username, g.getMatch(), g.isRoleHider());
                h.StartLobby();
                int timeout = 10;
                while (timeout-- > 0) {
                    Thread.Sleep(1000);
                }
                if (g.getMatch() != null)
                    h.startMatch(g.getMatch().getState());
                g = null;
            }
        }
    }

    public void updateNetwork() {
        if (h != null) 
            h.updateHost();
    }

    public void updateNetworkAnimator() {
        if (h != null) 
            h.updateNetworkAnimator();
        else if (g != null) 
            g.updateNetworkAnimator();            
    }

    public void Host() {
        g = null;
        h = new HostNetwork(username);
        h.StartLobby();
    }
    public void Guest(string hostIP) {
        h = null;
        g = new GuestNetwork(username);
        g.ConnectToHost(hostIP);
    }
    public void StartMatch() {
        if (h == null || !h.isSocketReady())
            return;
        h.startMatch();
    }

    public void checkSurvivedHiders() {
        if (h == null)
            return;
        h.checkSurvivedHiders();
    }

    public void sendPlayersDetailsAndNewHostIP() {
        if (h == null)
            return;
        h.sendPlayersDetailsAndNewHostIP();
    }
    public string[] getPlayerNames() {
        if (g == null && h == null)
            return null;
        string str = (g != null ? g.toString() : h.toString());
        return str.Split('/');
    }
    public string[] getPlayerStats() {
        if (g == null && h == null)
            return null;
        string str = (g != null ? g.toStringStats() : h.toStringStats());
        return str.Split('/');
    }
    public bool isNetworkReady() {
        if (g == null && h == null)
            return false;
        return (g != null ? g.isSocketReady() : h.isSocketReady());
    }
    public bool isGameReady() {
        if (g == null && h == null)
            return false;
        return (g != null ? g.isGameStarted() : h.isGameStarted());
    }
    public void setName(string username) {
        this.username = username;
    }
    public string getName() {
        return username;
    }
    public bool isHider() {
        if (g == null && h == null)
            return false;
        else if (h != null)
            return h.isRoleHider();
        else if (g != null)
            return g.isRoleHider();
        return false;
    }
    public MatchOrchestrator GetMatchOrchestrator() {
        if (g == null && h == null)
            return null;
        else if (h != null)
            return h.getMatch();
        else if (g != null)
            return g.getMatch();
        return null;
    }
    public void closeAll() {
        if (g == null && h == null)
            return;
        else if (h != null)
            h.terminateServer();
        else if (g != null)
            g.CloseSocket();
    }
    public bool resetEndGameFlag() {
        if (g == null && h == null)
            return false;
        else if (h != null)
            return h.resetEndGameFlag();
        else if (g != null)
            return g.resetEndGameFlag();
        return false;
    }
    public bool isCaught() {
        if (g == null && h == null)
            return true;
        else if (h != null)
            return h.getIsCaught();
        else if (g != null)
            return g.getIsCaught();
        return true;
    }
    public bool isHost() {
        return h != null;
    }
    public void resetCatches() {
        if (g == null && h == null)
            return;
        else if (h != null)
            h.resetCaught();
        else if (g != null)
            g.resetCaught();
    }
    public void registerCatch(string catchName) {
        if (g == null && h == null)
            return;
        else if (h != null)
            h.registerCatch(catchName);
        else if (g != null)
            g.registerCatch(catchName);
    }
    public void switchOwnRole() {
        if (g == null && h == null)
            return;
        else if (h != null)
            h.setRoleHider(!h.isRoleHider());
        else if (g != null)
            g.setRoleHider(!g.isRoleHider());
    }

    public PlayerNetworkController getPlayerNetworkController(string name) {
        if (g == null && h == null)
            return null;

        List<PlayerNetworkController> pncs;
        if (h != null) 
            pncs = h.GetPlayerNetworkControllers();
        else 
            pncs = g.GetPlayerNetworkControllers();

        if (pncs == null || pncs.Count == 0)
            return null;

        foreach (PlayerNetworkController pnc in pncs) {
            if (pnc.getGuestName().Equals(name)) {
                return pnc;
            }
        }
        return null;
    }

    public List<PlayerNetworkController> getPlayerNetworkControllers() {
        if (g == null && h == null)
            return null;

        if (h != null)
            return h.GetPlayerNetworkControllers();
        else
            return g.GetPlayerNetworkControllers();
    }

    public List<string> resetRemovedPlayersQueue() {
        if (g == null && h == null)
            return null;

        if (h != null)
            return h.resetRemovedPlayersQueue();
        else
            return g.resetRemovedPlayersQueue();
    }

    //Get IPv4
    public string IPv4() {

        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                return ip.ToString();
            }
        }

        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}
