window.initAnimatedTabs = () => {
    document.querySelectorAll('.custom-tabs-list').forEach(list => {
        const indicator = list.querySelector('.custom-tab-indicator');
        if (!indicator) return;

        const updateIndicator = (activeTab) => {
            if (!activeTab) return;
            const listRect = list.getBoundingClientRect();
            const tabRect = activeTab.getBoundingClientRect();
            
            list.style.setProperty('--active-tab-left', `${tabRect.left - listRect.left}px`);
            list.style.setProperty('--active-tab-top', `${tabRect.top - listRect.top}px`);
            list.style.setProperty('--active-tab-width', `${tabRect.width}px`);
            list.style.setProperty('--active-tab-height', `${tabRect.height}px`);
        };

        // Initial set based on currently active tab or nav link
        setTimeout(() => {
            const activeTab = list.querySelector('.custom-tab.active') || list.querySelector('.custom-tab.nav-link.active') || list.querySelector('.custom-tab[aria-selected="true"]');
            if (activeTab) {
                updateIndicator(activeTab);
                indicator.style.opacity = '1';
            }
        }, 50); // slight delay to allow layout to settle

        // Listen for clicks on the tabs to animate instantly
        list.querySelectorAll('.custom-tab').forEach(tab => {
            tab.addEventListener('click', () => {
                updateIndicator(tab);
            });
        });

        // Use MutationObserver to watch for active class changes (useful for Blazor NavLinks)
        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                if (mutation.attributeName === 'class') {
                    if (mutation.target.classList.contains('active')) {
                        updateIndicator(mutation.target);
                    }
                }
            });
        });

        list.querySelectorAll('.custom-tab').forEach(tab => {
            observer.observe(tab, { attributes: true });
        });

        // Handle window resize
        window.addEventListener('resize', () => {
            const activeTab = list.querySelector('.custom-tab.active');
            if (activeTab) updateIndicator(activeTab);
        });
    });
};

window.initGenerativeBackground = () => {
    const container = document.getElementById('bg-canvas-container');
    if (!container) return;
    if (!window.THREE) {
        setTimeout(window.initGenerativeBackground, 100);
        return;
    }

    // If a canvas is already running inside this exact container, do nothing
    if (container.querySelector('canvas') && window._bgInitialized) {
        return; 
    }

    // Cleanup any ghost canvases left by Blazor diffing
    while (container.firstChild) {
        container.removeChild(container.firstChild);
    }
    
    // Stop any previous running animation loop to prevent memory leaks
    if (window._bgAnimationId) {
        cancelAnimationFrame(window._bgAnimationId);
    }

    window._bgInitialized = true;

    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
    camera.position.z = 3;

    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    renderer.setSize(window.innerWidth, window.innerHeight);
    renderer.setPixelRatio(window.devicePixelRatio);
    
    // Explicitly add an ID to track the active canvas
    renderer.domElement.id = 'active-three-canvas';
    container.appendChild(renderer.domElement);

    const geometry = new THREE.IcosahedronGeometry(1.2, 64);
    const glowColor = new THREE.Color(0xf15a24);

    const material = new THREE.ShaderMaterial({
        uniforms: {
            time: { value: 0 },
            pointLightPos: { value: new THREE.Vector3(0, 0, 5) },
            color: { value: glowColor },
        },
        vertexShader: `
            uniform float time;
            varying vec3 vNormal;
            varying vec3 vPosition;
            vec3 mod289(vec3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            vec4 mod289(vec4 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            vec4 permute(vec4 x) { return mod289(((x*34.0)+1.0)*x); }
            vec4 taylorInvSqrt(vec4 r) { return 1.79284291400159 - 0.85373472095314 * r; }
            float snoise(vec3 v) {
                const vec2 C = vec2(1.0/6.0, 1.0/3.0);
                const vec4 D = vec4(0.0, 0.5, 1.0, 2.0);
                vec3 i = floor(v + dot(v, C.yyy));
                vec3 x0 = v - i + dot(i, C.xxx);
                vec3 g = step(x0.yzx, x0.xyz);
                vec3 l = 1.0 - g;
                vec3 i1 = min(g.xyz, l.zxy);
                vec3 i2 = max(g.xyz, l.zxy);
                vec3 x1 = x0 - i1 + C.xxx;
                vec3 x2 = x0 - i2 + C.yyy;
                vec3 x3 = x0 - D.yyy;
                i = mod289(i);
                vec4 p = permute(permute(permute(
                            i.z + vec4(0.0, i1.z, i2.z, 1.0))
                        + i.y + vec4(0.0, i1.y, i2.y, 1.0))
                        + i.x + vec4(0.0, i1.x, i2.x, 1.0));
                float n_ = 0.142857142857;
                vec3 ns = n_ * D.wyz - D.xzx;
                vec4 j = p - 49.0 * floor(p * ns.z * ns.z);
                vec4 x_ = floor(j * ns.z);
                vec4 y_ = floor(j - 7.0 * x_);
                vec4 x = x_ * ns.x + ns.yyyy;
                vec4 y = y_ * ns.x + ns.yyyy;
                vec4 h = 1.0 - abs(x) - abs(y);
                vec4 b0 = vec4(x.xy, y.xy);
                vec4 b1 = vec4(x.zw, y.zw);
                vec4 s0 = floor(b0) * 2.0 + 1.0;
                vec4 s1 = floor(b1) * 2.0 + 1.0;
                vec4 sh = -step(h, vec4(0.0));
                vec4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
                vec4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
                vec3 p0 = vec3(a0.xy, h.x);
                vec3 p1 = vec3(a0.zw, h.y);
                vec3 p2 = vec3(a1.xy, h.z);
                vec3 p3 = vec3(a1.zw, h.w);
                vec4 norm = taylorInvSqrt(vec4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
                p0 *= norm.x; p1 *= norm.y; p2 *= norm.z; p3 *= norm.w;
                vec4 m = max(0.6 - vec4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
                m = m * m;
                return 42.0 * dot(m * m, vec4(dot(p0, x0), dot(p1, x1), dot(p2, x2), dot(p3, x3)));
            }
            void main() {
                vNormal = normal;
                vPosition = position;
                float displacement = snoise(position * 2.0 + time * 0.5) * 0.2;
                vec3 newPosition = position + normal * displacement;
                gl_Position = projectionMatrix * modelViewMatrix * vec4(newPosition, 1.0);
            }
        `,
        fragmentShader: `
            uniform vec3 color;
            uniform vec3 pointLightPos;
            varying vec3 vNormal;
            varying vec3 vPosition;
            void main() {
                vec3 normal = normalize(vNormal);
                vec3 lightDir = normalize(pointLightPos - vPosition);
                float diffuse = max(dot(normal, lightDir), 0.0);
                float fresnel = 1.0 - dot(normal, vec3(0.0, 0.0, 1.0));
                fresnel = pow(fresnel, 2.0);
                vec3 finalColor = color * diffuse + color * fresnel * 0.5;
                gl_FragColor = vec4(finalColor, 1.0);
            }
        `,
        wireframe: true,
    });

    const mesh = new THREE.Mesh(geometry, material);
    mesh.position.x = 0.5;
    scene.add(mesh);

    const pointLight = new THREE.PointLight(0xffffff, 1, 100);
    pointLight.position.set(0, 0, 5);
    scene.add(pointLight);

    const animate = (t) => {
        // If the canvas was removed by Blazor, stop the loop to free memory
        if (!document.getElementById('active-three-canvas')) return;

        material.uniforms.time.value = t * 0.0003;
        mesh.rotation.y += 0.0005;
        mesh.rotation.x += 0.0002;
        renderer.render(scene, camera);
        window._bgAnimationId = requestAnimationFrame(animate);
    };
    
    // Start animation loop
    window._bgAnimationId = requestAnimationFrame(animate);

    // Only add listeners once globally
    if (!window._bgListenersAdded) {
        window._bgListenersAdded = true;
        window.addEventListener("resize", () => {
            camera.aspect = window.innerWidth / window.innerHeight;
            camera.updateProjectionMatrix();
            renderer.setSize(window.innerWidth, window.innerHeight);
        });

        window.addEventListener("mousemove", (e) => {
            const x = (e.clientX / window.innerWidth) * 2 - 1;
            const y = -(e.clientY / window.innerHeight) * 2 + 1;
            const vec = new THREE.Vector3(x, y, 0.5).unproject(camera);
            const dir = vec.sub(camera.position).normalize();
            const dist = -camera.position.z / dir.z;
            const pos = camera.position.clone().add(dir.multiplyScalar(dist));
            pointLight.position.copy(pos);
            material.uniforms.pointLightPos.value = pos;
        });
    }
};

// Animated stat counters — replicates Framer Motion useSpring counting
window.initAnimatedCounters = () => {
    document.querySelectorAll('.stat-value[data-target]').forEach(el => {
        const targetValue = parseFloat(el.dataset.target);
        const lastTarget = parseFloat(el.dataset.lastTarget || "-1");
        
        // Only skip if already animated AND the target hasn't changed
        if (el.dataset.animated === 'true' && targetValue === lastTarget) return;
        
        el.dataset.animated = 'true';
        el.dataset.lastTarget = targetValue;

        const duration = 2000;
        const startValue = lastTarget === -1 ? 0 : lastTarget;
        const startTime = performance.now();

        const easeOut = t => 1 - Math.pow(1 - t, 3);

        const tick = (now) => {
            const elapsed = now - startTime;
            const progress = Math.min(elapsed / duration, 1);
            const current = startValue + (easeOut(progress) * (targetValue - startValue));

            if (Number.isInteger(targetValue)) {
                el.textContent = Math.round(current).toLocaleString();
            } else {
                el.textContent = current.toFixed(1);
            }

            if (progress < 1) {
                requestAnimationFrame(tick);
            }
        };
        requestAnimationFrame(tick);
    });
};

// Initialize automatically on page load and Blazor enhanced navigation
const initializeUI = () => {
    if (typeof window.initGenerativeBackground === 'function') {
        window.initGenerativeBackground();
    }
    if (typeof window.initAnimatedTabs === 'function') {
        window.initAnimatedTabs();
    }
    if (typeof window.initAnimatedCounters === 'function') {
        window.initAnimatedCounters();
    }
};

document.addEventListener('DOMContentLoaded', initializeUI);
document.addEventListener('blazor:enhancedload', initializeUI);

// If DOMContentLoaded already fired (script loaded late), run immediately
if (document.readyState === 'interactive' || document.readyState === 'complete') {
    initializeUI();
}
