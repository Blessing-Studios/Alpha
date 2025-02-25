# Alpha

### Atual prioridade
    Sistema de Traits por equipamento quebrou, precisa arrumar

    Parece que foi resolvido !!Quando recarrega, o player reconectando pode pegar Autoridade de outros player characters

    Fazer sistema de pooling dos objetos online

    Criar sistema de interação mais complexo, para ter ter a opção de ter mais de um tipo de interação por objeto

    Quando um Personagem morrer, desligar as principais funções para ele não pesar no processamento

    Unificar o relógio interno do personagen para melhor controlar Buffs e Mana/Life Regen

    Arrumar lógica de personagens acertarem/aplicarem buffs  outros personagens (IA e Player acertando uns aos outros)

    

### Mudanças nas configurações
    Project Settings
        Physics Settings - Game Object
            Auto Sync Transforms -> False

### Observações
    Atualmente, a UI do inventário do Player e a UI do inventário do resto está separada, tab fecha o inventário do player, e interagir fecha o inventário do resto

    Network Animator tem problema para triggar a mesma animação várias vezes, contornar esse problema

    Separar a lógica das NetWorkBehaviors em duas classes, uma que é MonoBehaviors e outra que é NetworkBehaviors
        Exemplo PlayerController, transformar em duas classes

    Desacoplar as classes InventoryGrid e Inventory

    Criar um sistema de pooling https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/object-pooling/
    PS.: foi feito um sistema de pooling para a classe InventoryItem, precisa fazer para as outras
    

### Bugs para arrumar

Na classe PlayerCharacter, 
public override bool CheckIfAttackPressed(string nextComboAction)
Fazer essa função;

Criar lógica para checar se item foi duplicado

Arrumar a UI para funcionar em todas resoluções

Quando um jogador cai e reconecta a vida buga, o personagem não regenera mais vida

Personagens que já morreram bugam na Client do Player que entrou na partida depois que ele morreu

No modo online, se um bot ataca alguém, os players não host mostram erro

SpawnStateException: Object is not spawned
Unity.Netcode.NetworkSpawnManager.ChangeOwnership (Unity.Netcode.NetworkObject networkObject, System.UInt64 clientId, System.Boolean isAuthorized, System.Boolean isRequestApproval) (at ./Library/PackageCache/com.unity.netcode.gameobjects/Runtime/Spawning/NetworkSpawnManager.cs:623)
Unity.Netcode.NetworkObject.ChangeOwnership (System.UInt64 newOwnerClientId) (at ./Library/PackageCache/com.unity.netcode.gameobjects/Runtime/Core/NetworkObject.cs:1753)
Blessing.GameManager.GetOwnership (Unity.Netcode.NetworkObject networkObject) (at Assets/Scripts/GameManager.cs:146)
Blessing.Gameplay.TradeAndInventory.LooseItem.GetOwnership () (at Assets/Scripts/Gameplay/Trade&Inventory/Items/LooseItem.cs:102)
Blessing.Gameplay.TradeAndInventory.LooseItem.Interact (Blessing.Gameplay.Interation.Interactor interactor) (at 

public void GetOwnership(NetworkObject networkObject)
{
    ulong LocalClientId = NetworkManager.Singleton.LocalClientId;
    if (LocalClientId != networkObject.OwnerClientId)
        networkObject.ChangeOwnership(LocalClientId);
}


### Links para estudar
https://www.youtube.com/watch?v=SMWxCpLvrcc

https://www.youtube.com/watch?v=2ajD1GDbEzA vídeo tempo 46 min