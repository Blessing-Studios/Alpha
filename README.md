# Alpha

### Atual prioridade
    Fazer o sistema de status e equipamento

    !!Quando recarrega, o player reconectando pode pegar Autoridade de outros player characters

    Fazer sistema de equipamentos. // Criar tipos de equipamentos
    

### Mudanças nas configurações
    Project Settings
        Physics Settings - Game Object
            Auto Sync Transforms -> False

### Observações

    Network Animator tem problema para triggar a mesma animação várias vezes, contornar esse problema

    Precisa melhorar o jeito que manda os valores do dano para o personagem sendo atacado

    Separar a lógica das NetWorkBehaviors em duas classes, uma que é MonoBehaviors e outra que é NetworkBehaviors
        Exemplo PlayerController, transformar em duas classes

    Desacoplar as classes InventoryGrid e Inventory

    Mover CheckAvailableSpace e outros checks do InventoryGrid para Inventory

    Criar um sistema de pooling https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/object-pooling/
    

### Bugs para arrumar

Na classe PlayerCharacter, 
public override bool CheckIfAttackPressed(string nextComboAction)
Fazer essa função;

Criar lógica para checar se item foi duplicado

Mochilas estão bugando no online

Quando um jogador cai e reconecta a vida buga, o personagem não regenera mais vida



### Links para estudar
https://www.youtube.com/watch?v=SMWxCpLvrcc

https://www.youtube.com/watch?v=2ajD1GDbEzA vídeo tempo 46 min