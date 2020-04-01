using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    /// <summary>Sends a packet to a client via TCP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to a client via UDP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    /// <summary>Sends a packet to all clients via TCP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via TCP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }

    /// <summary>Sends a packet to all clients via UDP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via UDP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }

    #region Packets
    /// <summary>Sends a welcome message to the given client.</summary>
    /// <param name="_toClient">The client to send the packet to.</param>
    /// <param name="_msg">The message to send.</param>
    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Tells a client to spawn a player.</summary>
    /// <param name="_toClient">The client that should spawn the player.</param>
    /// <param name="_player">The player to spawn.</param>
    public static void SpawnPlayer(int _toClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);
            _packet.Write(_player.health);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Sends a player's updated position to all clients.</summary>
    /// <param name="_player">The player whose position to update.</param>
    public static void PlayerPosition(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);

            SendUDPDataToAll(_packet);
        }
    }

    /// <summary>Sends a player's updated rotation to all clients except to himself (to avoid overwriting the local player's rotation).</summary>
    /// <param name="_player">The player whose rotation to update.</param>
    public static void PlayerRotation(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.rotation);

            SendUDPDataToAll(_player.id, _packet);
        }
    }
    // player health
    public static void PlayerHealth(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerHealth))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.health);

            SendUDPDataToAll(_packet);
        }
    }
    // player ammo
    public static void PlayerAmmo(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerAmmo))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.ammo);

            SendUDPData(_player.id, _packet);
        }
    }
    // player ammo in mag
    public static void PlayerAmmoInMag(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerAmmoInMag))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.ammoInMag);

            SendUDPData(_player.id, _packet);
        }
    }
    // player Weapon
    public static void PlayerWeapon(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerWeapon))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.weaponId);

            SendUDPDataToAll(_packet);
        }
    }
    public static void PlayerSounds(Vector3 _vec3, int _soundIndex)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerSounds))
        {
            _packet.Write(_vec3);
            _packet.Write(_soundIndex);

            SendUDPDataToAll(_packet);
        }
    }
    public static void PlayerRemove(int _id)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRemove))
        {
            _packet.Write(_id);

            SendTCPDataToAll(_packet);
        }
    }
    public static void GameTimer(int timeLeft, string message)
    {
        using (Packet _packet = new Packet((int)ServerPackets.gameTimer))
        {
            _packet.Write(timeLeft);
            _packet.Write(message);

            SendUDPDataToAll(_packet);
        }
    }
    public static void KillFeed(string _nameKiller, string _nameKilled, int _weaponId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.killFeed))
        {
            _packet.Write(_nameKiller);
            _packet.Write(_nameKilled);
            _packet.Write(_weaponId);

            SendTCPDataToAll(_packet);
        }
    }
    public static void PlayerKillCount(int _id,int _killCount)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerKillCount))
        {
            _packet.Write(_killCount);

            SendUDPData(_id, _packet);
        }
    }
    public static void ZoneValues(Vector3 _zonePos,float _zoneScale)
    {
        using (Packet _packet = new Packet((int)ServerPackets.zoneValues))
        {
            _packet.Write(_zonePos);
            _packet.Write(_zoneScale);

            SendUDPDataToAll(_packet);
        }
    }
    public static void PPE(int _id, bool ppeBool)
    {
        using (Packet _packet = new Packet((int)ServerPackets.pPE))
        {
            _packet.Write(_id);
            _packet.Write(ppeBool);

            SendUDPData(_id, _packet);
        }
    }
    public static void Placement(int placement)
    {
        using (Packet _packet = new Packet((int)ServerPackets.placement))
        {
            _packet.Write(placement);

            SendUDPDataToAll(_packet);
        }
    }
    public static void InstatiateObject(int objectNum, Vector3 vec3)
    {
        using (Packet _packet = new Packet((int)ServerPackets.instatiateObj))
        {
            _packet.Write(objectNum);
            _packet.Write(vec3);

            SendUDPDataToAll(_packet);
        }
    }
    public static void PickableRemove(string name)
    {
        using (Packet _packet = new Packet((int)ServerPackets.pickableRemove))
        {
            _packet.Write(name);

            SendUDPDataToAll(_packet);
        }
    }
    #endregion
}
