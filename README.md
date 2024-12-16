# Alpha

### Atual prioridade
    Fazer o sistema de status e equipamento
    Fazer sistema de inventário, sincronizar os inventários online, criar lógica para pegar itens na classe inventário e colocar no InventoryGrid

    Quando o segundo player entra, ele pega o PlayerInventoryGrid de todo mundo

### Mudanças nas configurações
    Project Settings
        Physics Settings - Game Object
            Auto Sync Transforms -> False

### Observações

    Network Animator tem problema para triggar a mesma animação várias vezes, contornar esse problema

    Precisa melhorar o jeito que manda os valores do dano para o personagem sendo atacado

    Separar a lógica das NetWorkBehaviors em duas classes, uma que é MonoBehaviors e outra que é NetworkBehaviors
        Exemplo PlayerController, transformar em duas classes
    

### Bugs para arrumar

Na classe PlayerCharacter, 
public override bool CheckIfAttackPressed(string nextComboAction)
Fazer essa função;
Quando um jogador cai e reconecta a vida buga, o personagem não regenera mais vida


### Links para estudar
https://www.youtube.com/watch?v=SMWxCpLvrcc

https://www.youtube.com/watch?v=2ajD1GDbEzA vídeo tempo 46 min