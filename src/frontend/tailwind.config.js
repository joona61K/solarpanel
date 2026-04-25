/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './app/**/*.{js,ts,jsx,tsx,mdx}',
    './components/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      colors: {
        solar: {
          yellow: '#f59e0b',
          green: '#10b981',
          blue: '#3b82f6',
          red: '#ef4444',
        },
      },
    },
  },
  plugins: [],
};
