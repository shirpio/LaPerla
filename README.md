# 🥖 Sistema de Ventas - La Perla

Sistema web desarrollado para la gestión de productos, ventas y reportes de una panadería/confitería.

---

# 🚀 Demo Online

🔗 https://laperla-production.up.railway.app

## Usuario DEMO

Email:
```txt
demo@laperla.com
```

Contraseña:
```txt
demo123
```

⚠️ La demo pública tiene fines de portfolio.  
Los datos pueden reiniciarse periódicamente.

---

# ✨ Funcionalidades

✅ Gestión de productos  
✅ Registro de ventas  
✅ Productos por unidad y kilogramo  
✅ Edición y eliminación antes de confirmar venta  
✅ Reportes por rango de fechas  
✅ Dashboard con gráficos estadísticos  
✅ Exportación PDF profesional  
✅ Login protegido con Identity  
✅ Diseño responsive para celular y PC  

---

# 📸 Capturas del sistema

## Login

<img width="1917" height="879" alt="image" src="https://github.com/user-attachments/assets/6eafe3af-c90b-40f6-96ad-544d23456723" />


---

## Gestión de Productos

<img width="881" height="879" alt="image" src="https://github.com/user-attachments/assets/6eb2924c-a8be-4867-aac6-020c13aa9dcd" />


---

## Registro de Ventas

<img width="1920" height="877" alt="image" src="https://github.com/user-attachments/assets/be6e789c-39d1-4daf-a5c7-53e3ef9f44bc" />


---

## Dashboard y Reportes

<img width="1920" height="874" alt="image" src="https://github.com/user-attachments/assets/816d7673-925c-4222-aecb-b3691adc570b" />
<img width="1920" height="868" alt="image" src="https://github.com/user-attachments/assets/56d0ffe4-82a7-4c47-87ed-d3f4180022fd" />
<img width="1920" height="879" alt="image" src="https://github.com/user-attachments/assets/95d76475-640c-4024-a9dd-6512a8dc7b54" />


---

## Exportación PDF

<img width="557" height="786" alt="image" src="https://github.com/user-attachments/assets/2d017614-4c80-46bf-ab02-1ad0104bb2e1" />


---

# 📊 Reportes y Estadísticas

El sistema incluye:

- Total de ventas por día
- Productos más vendidos
- Gráficos dinámicos
- Reportes por rango de fechas
- Exportación PDF estilizada

---

# 📦 Importación de Productos mediante Excel

El sistema permite cargar productos utilizando una plantilla Excel.

## Formato esperado

| Nombre | Precio | TipoVenta |
|---|---|---|
| Pan | 1200 | Unidad |
| Criollos | 3500 | Kilogramo |

## Tipos válidos

- Unidad
- Kilogramo
- Paquete
---
## Excel valido

[PanaderiaLaPerla.xlsx](https://github.com/user-attachments/files/28032374/PanaderiaLaPerla.xlsx)

---

# 🛠 Tecnologías utilizadas

- ASP.NET Core MVC
- Entity Framework Core
- PostgreSQL
- Railway
- Bootstrap 5
- Chart.js
- QuestPDF
- ASP.NET Identity

---

# ⚙️ Instalación local

## 1. Clonar repositorio

```bash
git clone URL_DEL_REPOSITORIO
```

---

## 2. Configurar conexión

Editar:

```txt
appsettings.json
```

Agregar:

```json
"ConnectionStrings": {
  "DefaultConnection": "TU_CONEXION"
}
```

---

## 3. Ejecutar migraciones

```bash
dotnet ef database update
```

---

## 4. Ejecutar proyecto

```bash
dotnet run
```

---

# ☁️ Deploy

Aplicación desplegada en:

- Railway
- PostgreSQL Cloud

---

# 👨‍💻 Autor

Agustin Balbastro

Desarrollado como proyecto portfolio utilizando ASP.NET Core MVC.
