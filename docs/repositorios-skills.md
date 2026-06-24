# Dónde encontrar skills de ejemplo

> Hoja de recursos para el curso de GitHub Copilot — **Agent Skills (`SKILL.md`)**

Una *skill* es una carpeta con un archivo `SKILL.md`: instrucciones en lenguaje claro que el agente (Copilot, Claude Code, Codex…) lee **solo cuando hacen falta**. No es código, son instrucciones. La mejor forma de aprender a crearlas es **leer las de otros**. Aquí tienes dónde.

---

## 1. Empieza por aquí (oficiales y fiables)

- **anthropics/skills** — https://github.com/anthropics/skills
  El repositorio de referencia. Incluye una `template-skill` para copiar y skills reales (`skill-creator`, `frontend-design`…). Empieza leyendo 3 o 4 de estas.

- **github/awesome-copilot** — https://github.com/github/awesome-copilot
  La colección de GitHub, compatible con Copilot directamente. Se instala con `gh skills install github/awesome-copilot <nombre>` o copiando la carpeta a mano.

---

## 2. Colecciones grandes y curadas (para ver variedad)

- **VoltAgent/awesome-agent-skills** — https://github.com/VoltAgent/awesome-agent-skills
  Más de 1000 skills de equipos oficiales (Anthropic, Google, Vercel, Stripe, Cloudflare, Figma…) y de la comunidad. Seleccionadas a mano, no generadas en masa.

- **heilcheng/awesome-agent-skills** — https://github.com/heilcheng/awesome-agent-skills
  Tutoriales, guías y directorios de skills. Buen punto de entrada con explicaciones.

- **openai/skills** — https://github.com/openai/skills
  Las del ecosistema Codex (OpenAI). Mismo formato `SKILL.md`: sirve para mostrar que el estándar es común a todos los agentes.

- **GoogleChrome/modern-web-guidance** — https://github.com/GoogleChrome/modern-web-guidance
  Skills oficiales del equipo de Chrome (accesibilidad, rendimiento, web moderna).

---

## 3. Buscadores y directorios

- **skills.sh** — https://skills.sh
  Leaderboard de Vercel. Para ver de un vistazo los repos de skills más populares y sus estadísticas de uso.

- **agentskills.io** — https://agentskills.io
  La web del estándar abierto. Aquí está la especificación oficial del formato.

---

## 4. Instalar sin copiar carpetas a mano (CLI)

- **vercel-labs/skills** (`npx skills`) — https://github.com/vercel-labs/skills
  Funciona como un "gestor de paquetes" de skills. Detecta qué agentes tienes y coloca los archivos en la ruta correcta.

  ```bash
  npx skills find <búsqueda>      # buscar skills
  npx skills add <owner/repo>     # instalar
  npx skills list                 # ver instaladas
  npx skills update               # actualizar
  ```

- **gh skill** (GitHub CLI, v2.90.0+)

  ```bash
  gh skill preview <owner/repo> <skill>   # INSPECCIONAR antes de instalar
  gh skill install <owner/repo> <skill>   # instalar
  ```

---

## 5. En español

- **crucenojmc/ia-agents-and-skills** — https://github.com/crucenojmc/ia-agents-and-skills
  Recopilatorio en castellano que enlaza a `skills.sh`, las skills de Anthropic, `awesome-copilot`, *Gentleman-Skills* y *clean-code-skills*.

- **Web Reactiva** — guía de skills para programadores: https://www.webreactiva.com/blog/skills-programadores-agentes-ia
  Explicación clara del CLI `npx skills` y de cómo se estructura un `SKILL.md`, en español.

---

## ⚠️ Antes de instalar nada: seguridad

Las skills **no las verifica nadie por ti**. Un `SKILL.md` puede contener instrucciones ocultas o scripts maliciosos. Regla de oro:

> **Inspecciona siempre el contenido antes de instalar** — con `gh skill preview` o, simplemente, abriendo el `SKILL.md` y leyéndolo.

No instales a ciegas. Si no entiendes lo que hace una skill, no la uses.

---

## Recordatorio: dónde van las skills

| Ámbito | Ruta | Para qué |
|---|---|---|
| Proyecto (repo) | `.github/skills/<nombre>/SKILL.md` | Compartida con el equipo vía Git |
| Proyecto (compat. Claude) | `.claude/skills/<nombre>/SKILL.md` | Copilot también las lee |
| Personal | `~/.copilot/skills/` o `~/.agents/skills/` | Tuyas, en todos tus proyectos |

Cada `SKILL.md` necesita como mínimo un *frontmatter* YAML con `name` y `description`, y debajo las instrucciones en Markdown. La `description` es lo que decide **cuándo** se activa: escríbela con claridad.

---

## Tabla resumen

| Recurso | Tipo | Enlace |
|---|---|---|
| anthropics/skills | Oficial · referencia | github.com/anthropics/skills |
| github/awesome-copilot | Colección · Copilot | github.com/github/awesome-copilot |
| VoltAgent/awesome-agent-skills | Colección grande | github.com/VoltAgent/awesome-agent-skills |
| heilcheng/awesome-agent-skills | Guías y directorios | github.com/heilcheng/awesome-agent-skills |
| openai/skills | Oficial · Codex | github.com/openai/skills |
| GoogleChrome/modern-web-guidance | Oficial · Chrome | github.com/GoogleChrome/modern-web-guidance |
| skills.sh | Buscador | skills.sh |
| agentskills.io | Especificación | agentskills.io |
| vercel-labs/skills | CLI | github.com/vercel-labs/skills |
| crucenojmc/ia-agents-and-skills | En español | github.com/crucenojmc/ia-agents-and-skills |
