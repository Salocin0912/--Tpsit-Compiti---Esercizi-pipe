#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <time.h>
#include <sys/wait.h>

int main(void) {
    int pipe1[2]; // A -> B
    int pipe2[2]; // B -> C
    
    if (pipe(pipe1) == -1 || pipe(pipe2) == -1) {
        perror("pipe");
        exit(-1);
    }
    
    // Generazione del primo figlio (B)
    pid_t pid_B = fork();
    if (pid_B == -1) {
        perror("fork B");
        exit(-2);
    }
    
    if (pid_B == 0) {
        // --- PROCESSO B ---
        close(pipe1[1]); // Non scrive su pipe1
        close(pipe2[0]); // Non legge da pipe2
        close(pipe2[1]); // Chiusura temporanea (verrà riaperta o gestita, qui chiudiamo pipe2[0] e usiamo pipe2[1] per scrivere)
        // Nota: per pulizia, chiudiamo i lati non usati di B:
        // Usa pipe1[0] per leggere da A, pipe2[1] per scrivere a C.
        // Riapriamo il corretto flusso chiudendo i descrittori avanzati:
        // pipe1[1] già chiuso sopra. Chiudiamo pipe2[0] sopra.
        
        int num;
        // Legge uno alla volta i numeri da A tramite pipe1
        while (read(pipe1[0], &num, sizeof(int)) > 0) {
            // Scarta i numeri minori di 50
            if (num >= 50) {
                int quadrato = num * num;
                // Invia il quadrato al processo C tramite pipe2
                write(pipe2[1], &quadrato, sizeof(int));
            }
        }
        
        close(pipe1[0]);
        close(pipe2[1]);
        exit(0);
    }
    
    // Generazione del secondo figlio (C)
    pid_t pid_C = fork();
    if (pid_C == -1) {
        perror("fork C");
        exit(-2);
    }
    
    if (pid_C == 0) {
        // --- PROCESSO C ---
        close(pipe1[0]); // Non usa pipe1
        close(pipe1[1]);
        close(pipe2[1]); // Non scrive su pipe2, legge solo
        
        int num;
        int somma = 0;
        int conteggio = 0;
        
        printf("Numeri ricevuti ed elaborati (processo C):\n");
        // Legge i numeri inviati da B finché la pipe rimane aperta
        while (read(pipe2[0], &num, sizeof(int)) > 0) {
            printf("%d ", num);
            somma += num;
            conteggio++;
        }
        printf("\n");
        
        // Calcola e stampa la media aritmetica se ci sono dati
        if (conteggio > 0) {
            double media = (double)somma / conteggio;
            printf("Media aritmetica: %.2f\n", media);
        } else {
            printf("Nessun numero maggiore o uguale a 50 ricevuto.\n");
        }
        
        close(pipe2[0]);
        exit(0);
    }
    
    // --- PROCESSO PADRE (A) ---
    close(pipe1[0]); // Non legge da pipe1
    close(pipe2[0]); // Non legge da pipe2
    close(pipe2[1]); // Non scrive su pipe2
    
    // Inizializza il generatore di numeri casuali
    srand(time(NULL));
    
    // Genera 20 numeri interi casuali tra 1 e 100 e li invia a B
    for (int i = 0; i < 20; i++) {
        int casuale = (rand() % 100) + 1;
        write(pipe1[1], &casuale, sizeof(int));
    }
    
    // Chiude la scrittura su pipe1 per segnalare la fine dei dati a B
    close(pipe1[1]);
    
    // Attende il completamento di entrambi i figli
    wait(NULL);
    wait(NULL);
    
    return 0;
}