#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/wait.h>

int main(void) {
    // Array di 10 interi
    int arr[10] = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
    
    // fd[0] -> lettura, fd[1] -> scrittura
    int fd[2];
    
    if (pipe(fd) == -1) {
        perror("pipe");
        exit(-1);
    }
    
    pid_t ritorno_fork = fork();
    if (ritorno_fork == -1) {
        perror("fork");
        exit(-2);
    }
    
    // Processo Figlio: calcola la somma dei numeri pari
    if (!ritorno_fork) {
        close(fd[0]); // Chiudiamo il lato di lettura non usato
        
        int somma_pari = 0;
        for (int i = 0; i < 10; i++) {
            if (arr[i] % 2 == 0) {
                somma_pari += arr[i];
            }
        }
        
        // Invia il risultato al padre tramite la pipe
        write(fd[1], &somma_pari, sizeof(somma_pari));
        close(fd[1]);
        
        exit(0);
    }
    
    // Processo Padre: calcola la somma dei dispari e legge il risultato del figlio
    close(fd[1]); // Chiudiamo il lato di scrittura non usato
    
    int somma_dispari = 0;
    for (int i = 0; i < 10; i++) {
        if (arr[i] % 2 != 0) {
            somma_dispari += arr[i];
        }
    }
    
    int somma_pari_ricevuta = 0;
    // Legge il risultato inviato dal figlio
    read(fd[0], &somma_pari_ricevuta, sizeof(somma_pari_ricevuta));
    close(fd[0]);
    
    // Attende la terminazione del figlio
    wait(NULL);
    
    // Calcola la somma totale e stampa i risultati
    int somma_totale = somma_pari_ricevuta + somma_dispari;
    
    printf("Somma pari (dal figlio): %d\n", somma_pari_ricevuta);
    printf("Somma dispari (dal padre): %d\n", somma_dispari);
    printf("Somma totale complessiva: %d\n", somma_totale);
    
    return 0;
}