#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <ctype.h>
#include <string.h>
#include <sys/wait.h>

// Definizione della struttura richiesta per il passaggio dati
struct Messaggio {
    char stringa_maiuscola[100];
    int num_vocali;
};

int main(void) {
    int pipe1[2]; // Padre -> Figlio
    int pipe2[2]; // Figlio -> Padre
    
    if (pipe(pipe1) == -1 || pipe(pipe2) == -1) {
        perror("pipe");
        exit(-1);
    }
    
    pid_t ritorno_fork = fork();
    if (ritorno_fork == -1) {
        perror("fork");
        exit(-2);
    }
    
    // Processo Figlio
    if (!ritorno_fork) {
        close(pipe1[1]); // Chiudiamo la scrittura di pipe1
        close(pipe2[0]); // Chiudiamo la lettura di pipe2
        
        char buffer[100];
        // Legge la stringa inviata dal padre
        read(pipe1[0], buffer, sizeof(buffer));
        close(pipe1[0]);
        
        struct Messaggio msg;
        msg.num_vocali = 0;
        
        // Trasforma in maiuscolo e conta le vocali
        for (int i = 0; buffer[i] != '\0'; i++) {
            msg.stringa_maiuscola[i] = toupper(buffer[i]);
            
            char c = tolower(buffer[i]);
            if (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u') {
                msg.num_vocali++;
            }
            msg.stringa_maiuscola[i + 1] = '\0'; // Termina la stringa
        }
        
        // Invia la struttura compilata al padre tramite pipe2
        write(pipe2[1], &msg, sizeof(struct Messaggio));
        close(pipe2[1]);
        
        exit(0);
    }
    
    // Processo Padre
    close(pipe1[0]); // Chiudiamo la lettura di pipe1
    close(pipe2[1]); // Chiudiamo la scrittura di pipe2
    
    char input[100];
    printf("Inserisci una stringa di testo: ");
    fgets(input, sizeof(input), stdin);
    
    // Rimuove eventuale newline finale inserito da fgets
    input[strcspn(input, "\n")] = 0;
    
    // Invia la stringa al figlio tramite pipe1
    write(pipe1[1], input, sizeof(input));
    close(pipe1[1]);
    
    // Attende la terminazione del figlio
    wait(NULL);
    
    struct Messaggio ris;
    // Legge la struttura dal figlio tramite pipe2
    read(pipe2[0], &ris, sizeof(struct Messaggio));
    close(pipe2[0]);
    
    // Stampa i risultati finali
    printf("Stringa in maiuscolo: %s\n", ris.stringa_maiuscola);
    printf("Numero di vocali: %d\n", ris.num_vocali);
    
    return 0;
}