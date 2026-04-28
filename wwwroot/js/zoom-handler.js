(function() {
    let browserZoom = 100;
    let visualZoom = 100;
    
    // Detectar zoom del navegador (basado en devicePixelRatio)
    function detectBrowserZoom() {
        // En navegadores modernos
        if (window.devicePixelRatio) {
            return Math.round(window.devicePixelRatio * 100);
        }
        
        // Fallback para navegadores más antiguos
        return Math.round((window.outerWidth / window.innerWidth) * 100);
    }

    // Detectar zoom visual (trackpad/pinch)
    function detectVisualZoom() {
        // Usar el tamaño de fuente del documento como referencia
        const fontSize = parseFloat(getComputedStyle(document.documentElement).fontSize);
        const baseSize = 16; // Tamaño base
        return (fontSize / baseSize) * 100;
    }

    // Aplicar ajustes combinados
    function applyZoomAdjustment() {
        const browser = detectBrowserZoom();
        const visual = detectVisualZoom();
        const combined = (browser / 100) * (visual / 100);
        
        const root = document.documentElement;
        
        console.log('Browser Zoom:', browser + '%', 'Visual Zoom:', visual + '%', 'Combined:', Math.round(combined * 100) + '%');
        
        if (combined >= 1.2) {
            // Zoom IN (120% o más)
            root.style.setProperty('--font-scale', '0.80');
            root.style.setProperty('--padding-scale', '0.80');
            root.style.setProperty('--gap-scale', '0.80');
            document.body.classList.add('zoom-in');
            document.body.classList.remove('zoom-out', 'zoom-normal');
        } else if (combined <= 0.8) {
            // Zoom OUT (80% o menos)
            root.style.setProperty('--font-scale', '1.20');
            root.style.setProperty('--padding-scale', '1.20');
            root.style.setProperty('--gap-scale', '1.20');
            document.body.classList.add('zoom-out');
            document.body.classList.remove('zoom-in', 'zoom-normal');
        } else {
            // Zoom NORMAL (80% - 120%)
            root.style.setProperty('--font-scale', '1');
            root.style.setProperty('--padding-scale', '1');
            root.style.setProperty('--gap-scale', '1');
            document.body.classList.add('zoom-normal');
            document.body.classList.remove('zoom-in', 'zoom-out');
        }
    }

    // Escuchar cambios de tamaño (zoom del navegador)
    let resizeTimeout;
    window.addEventListener('resize', function() {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(function() {
            applyZoomAdjustment();
        }, 100);
    });

    // Escuchar cambios de zoom visual (trackpad, pinch, rueda)
    let wheelTimeout;
    document.addEventListener('wheel', function(event) {
        // Detectar cualquier evento wheel (incluye trackpad)
        clearTimeout(wheelTimeout);
        wheelTimeout = setTimeout(function() {
            applyZoomAdjustment();
        }, 150);
    }, { passive: true });

    // Para Safari - gesturechange
    document.addEventListener('gesturechange', function(event) {
        setTimeout(function() {
            applyZoomAdjustment();
        }, 150);
    }, false);

    // Pinch zoom en móviles
    let lastDistance = 0;
    document.addEventListener('touchmove', function(event) {
        if (event.touches.length === 2) {
            const touch1 = event.touches[0];
            const touch2 = event.touches[1];
            
            const distance = Math.hypot(
                touch2.clientX - touch1.clientX,
                touch2.clientY - touch1.clientY
            );
            
            if (lastDistance > 0 && Math.abs(distance - lastDistance) > 10) {
                applyZoomAdjustment();
            }
            lastDistance = distance;
        }
    }, { passive: true });

    document.addEventListener('touchend', function() {
        lastDistance = 0;
        setTimeout(function() {
            applyZoomAdjustment();
        }, 100);
    }, false);

    // Observer para detectar cambios de tamaño del documento
    const resizeObserver = new ResizeObserver(function(entries) {
        applyZoomAdjustment();
    });

    resizeObserver.observe(document.documentElement);

    // Escuchar orientación en móviles
    window.addEventListener('orientationchange', function() {
        setTimeout(function() {
            applyZoomAdjustment();
        }, 200);
    });

    // Inicializar
    window.addEventListener('load', function() {
        setTimeout(function() {
            applyZoomAdjustment();
        }, 500);
    });

    if (document.readyState !== 'loading') {
        setTimeout(function() {
            applyZoomAdjustment();
        }, 500);
    } else {
        document.addEventListener('DOMContentLoaded', function() {
            setTimeout(function() {
                applyZoomAdjustment();
            }, 500);
        });
    }

    // Función de debug
    window.debugZoom = function() {
        console.log('=== ZOOM DEBUG ===');
        console.log('Browser Zoom:', detectBrowserZoom() + '%');
        console.log('Visual Zoom:', detectVisualZoom() + '%');
        console.log('Device Pixel Ratio:', window.devicePixelRatio);
        console.log('Font Scale:', getComputedStyle(document.documentElement).getPropertyValue('--font-scale'));
        console.log('Inner Width:', window.innerWidth);
        console.log('Outer Width:', window.outerWidth);
        console.log('================');
    };

    // Iniciar debug cada 2 segundos durante carga
    let debugInterval = setInterval(function() {
        debugZoom();
    }, 2000);

    window.addEventListener('load', function() {
        setTimeout(function() {
            clearInterval(debugInterval);
        }, 5000);
    });
})();