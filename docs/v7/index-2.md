# **Verifieringsritual – Säkerställ att kedjan fungerar**


---

## **Syfte**

Bekräfta att hela AuthN/AuthZ‑flödet fungerar:  
**Token → Policy → API‑svar → UI‑reaktion**

***

## **Testa i ordning**

### **1) 401 – Ingen identitet**

*   Ta bort token / logga ut
*   Anropa en skyddad endpoint (t.ex. )
*   Förväntat svar: **401 Unauthorized**
*   UI ska visa *”Logga in”*

***

### **2) 403 – Fel identitet**

*   Logga in som **Student** eller **Lärare** (fel roll)
*   Försök utföra en Admin‑åtgärd
*   Förväntat svar: **403 Forbidden**
*   UI ska visa *”Behörighet saknas”*

***

### **3) 2xx – Rätt identitet**

*   Logga in som **Admin**
*   Utför samma åtgärd
*   Förväntat svar: **201/204**
*   UI ska visa resultatet utan fel

***

## **Om något går fel**

*   Kontrollera **middleware‑ordningen**
*   Kontrollera **stavning av roller/policies**
*   Kontrollera att UI faktiskt **skickar token**
*   Kontrollera **Issuer/Audience/Key**
