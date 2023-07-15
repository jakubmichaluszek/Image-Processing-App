; Jakub Michaluszek
; Grupa 1
; Filtr medianowy
; Zwraca medianę w rejestrze RAX
.data

.code
; Procedura wylicza medianę wartości z przekazanej tablicy 'values' 
; Parametry wejsciowe wskaźnik na tablicę values, oraz długość tablicy, przechowywane w rejestrach RCX oraz RDX
; Parametry wyjsciowe mediana z przekazanej tablicy, przechowywana w rejestrze RAX

; przyjmuje dwa argumenty: wskaźnik na tablicę oraz jej długość
GetMedian PROC EXPORT
    ; przekazane argumenty są już dostępne w rejestrach RCX i RDX
    mov r8, rcx   ; wskaźnik na tablicę jest przekazany w rejestrze RCX
    mov r9, rdx   ; długość tablicy jest przekazana w rejestrze RDX

    ; oblicz indeks środkowego elementu tablicy
    mov rax, r9
    inc rax
    shr rax, 1
    mov r10, rax

    ; sprawdź, czy długość tablicy jest parzysta
    test r9, 1
    jz parzysta

    ; jeśli długość jest nieparzysta, zwróć wartość środkowego elementu
    mov rax, [r8 + r10 * 4]
    ret

parzysta:
    ; jeśli długość jest parzysta, zwróć średnią wartość dwóch środkowych elementów
    mov rax, [r8 + r10 * 4]
    add rax, [r8 + r10 * 4 - 4]
    shr rax, 1
    ret
GetMedian ENDP
end