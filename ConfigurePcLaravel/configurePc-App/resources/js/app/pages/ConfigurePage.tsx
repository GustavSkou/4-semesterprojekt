import { useState } from 'react';
import {
  CheckCircle2, XCircle, ChevronRight,
  AlertTriangle, Trash2, Info,
} from 'lucide-react';
import { useNavigate } from 'react-router';
import { categories, PCComponent, checkCompatibility, getTotalPowerDraw } from '../data/components';
import { useApp } from '../context/AppContext';
import { categoryIcons } from '../constants/categoryIcons';

function ComponentCard({
  component,
  isSelected,
  isCompatible,
  incompatibilityReasons,
  onSelect,
}: {
  component: PCComponent;
  isSelected: boolean;
  isCompatible: boolean;
  incompatibilityReasons: string[];
  onSelect: () => void;
}) {
  const [showTooltip, setShowTooltip] = useState(false);

  return (
    <div
      onClick={isCompatible ? onSelect : undefined}
      className={[
        'relative rounded-xl border transition-all duration-150 flex flex-col',
        isSelected
          ? 'bg-blue-selected border-[1.5px] border-blue-500'
          : isCompatible
          ? 'bg-surface-card border-line-card'
          : 'bg-surface-muted border-line-faint',
        isCompatible ? 'cursor-pointer' : 'cursor-not-allowed opacity-45',
      ].join(' ')}
    >
      {/* Compatibility badge */}
      <div
        className="absolute top-3 right-3"
        onMouseEnter={() => !isCompatible && setShowTooltip(true)}
        onMouseLeave={() => setShowTooltip(false)}
      >
        {isSelected ? (
          <CheckCircle2 size={16} className="text-green-500" />
        ) : isCompatible ? (
          <div className="w-4 h-4" />
        ) : (
          <div className="relative">
            <XCircle size={16} className="text-red-500" />
            {showTooltip && incompatibilityReasons.length > 0 && (
              <div className="absolute right-0 top-6 z-50 rounded-lg p-3 text-xs w-56 bg-surface-tooltip border border-blue-tooltip-border text-slate-100">
                <p className="font-medium mb-1.5 text-red-400">Incompatible</p>
                <ul className="space-y-1">
                  {incompatibilityReasons.map((r, i) => (
                    <li key={i} className="flex gap-1.5 text-slate-400">
                      <span className="mt-0.5 flex-shrink-0">•</span>
                      <span>{r}</span>
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        )}
      </div>

      <div className="p-4 flex flex-col gap-3 flex-1">
        {/* Brand + Name */}
        <div>
          <p className="text-xs mb-0.5 text-slate-500">{component.brand}</p>
          <p className={`text-sm leading-snug pr-5 ${isCompatible ? 'text-slate-200' : 'text-gray-600'}`}>
            {component.name}
          </p>
        </div>

        {/* Specs */}
        <div className="flex flex-col gap-1 flex-1">
          {component.specs.slice(0, 3).map((spec, i) => (
            <div key={spec.label + i} className="flex justify-between gap-2">
              <span className="text-xs text-gray-600">{spec.label}</span>
              <span className={`text-xs text-right ${isCompatible ? 'text-slate-400' : 'text-gray-700'}`}>
                {spec.value}
              </span>
            </div>
          ))}
          {component.powerDraw > 0 && (
            <div className="flex justify-between gap-2">
              <span className="text-xs text-gray-600">Power</span>
              <span className={`text-xs ${isCompatible ? 'text-amber-500' : 'text-gray-700'}`}>
                {component.powerDraw} W
              </span>
            </div>
          )}
        </div>

        {/* Price + Action */}
        <div className="flex items-center justify-between pt-2 border-t border-line-faint">
          <span className={`font-semibold text-sm ${isCompatible ? 'text-blue-400' : 'text-gray-700'}`}>
            ${component.price.toLocaleString()}
          </span>
          {isCompatible && (
            <button
              className={`text-xs px-3 py-1 rounded-lg transition-all ${
                isSelected
                  ? 'bg-blue-700 text-blue-200'
                  : 'bg-slate-800 text-blue-300 border border-blue-border'
              }`}
            >
              {isSelected ? 'Selected ✓' : 'Select'}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

export function ConfigurePage() {
  const { selectedComponents, selectComponent, deselectComponent, components } = useApp();
  const [activeCategory, setActiveCategory] = useState('cpu');
  const navigate = useNavigate();

  const totalPower = getTotalPowerDraw(selectedComponents);
  const psuWattage = selectedComponents['psu']?.psuWattage ?? 0;
  const totalPrice = Object.values(selectedComponents).reduce(
    (sum, c) => sum + (c?.price ?? 0), 0
  );
  const selectedCount = Object.values(selectedComponents).filter(Boolean).length;
  const powerPercent = psuWattage > 0 ? Math.min((totalPower / psuWattage) * 100, 100) : 0;

  const activeCategoryComponents = components.filter(c => c.categoryId === activeCategory);

  const handleSelect = (component: PCComponent) => {
    if (selectedComponents[component.categoryId]?.id === component.id) {
      deselectComponent(component.categoryId);
    } else {
      selectComponent(component);
    }
  };

  const allCompatibilityIssues: string[] = [];
  Object.values(selectedComponents).forEach(c => {
    if (c) {
      const others = { ...selectedComponents, [c.categoryId]: null };
      const { reasons } = checkCompatibility(c, others);
      reasons.forEach(r => allCompatibilityIssues.push(r));
    }
  });

  return (
    <div className="flex h-[calc(100vh-64px)] overflow-hidden">
      {/* ───────────────────── LEFT PANEL ───────────────────── */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Page header */}
        <div className="px-6 pt-5 pb-4 border-b border-line">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-lg text-slate-200">Configure Your PC</h1>
              <p className="text-xs mt-0.5 text-slate-600">
                {selectedCount} of {categories.length} components selected
              </p>
            </div>
            <div className="flex items-center gap-2">
              <div className="flex items-center gap-1.5 text-xs px-3 py-1.5 rounded-lg bg-line text-slate-600">
                <Info size={12} />
                Hover incompatible components for details
              </div>
            </div>
          </div>
          {/* Category tabs */}
          <div className="flex gap-2 mt-4 overflow-x-auto pb-1 scrollbar-hide">
            {categories.map(cat => {
              const sel = selectedComponents[cat.id];
              const catComponents = components.filter(c => c.categoryId === cat.id);
              const compatibleCount = catComponents.filter(c => {
                const { compatible } = checkCompatibility(c, selectedComponents);
                return compatible;
              }).length;

              return (
                <button
                  key={cat.id}
                  onClick={() => setActiveCategory(cat.id)}
                  className={`flex items-center gap-1.5 px-3 py-2 rounded-lg text-xs whitespace-nowrap flex-shrink-0 transition-all border ${
                    activeCategory === cat.id
                      ? 'bg-blue-active text-blue-300 border-blue-tab-border'
                      : sel
                      ? 'bg-green-surface text-green-400 border-green-800'
                      : 'bg-surface-card text-slate-600 border-line'
                  }`}
                >
                  {categoryIcons[cat.id]}
                  {cat.name}
                  {sel ? (
                    <CheckCircle2 size={11} className="text-green-500" />
                  ) : (
                    <span className="rounded-full px-1 text-[10px] bg-line text-gray-700">
                      {compatibleCount}
                    </span>
                  )}
                </button>
              );
            })}
          </div>
        </div>

        {/* Currently selected for this category */}
        {selectedComponents[activeCategory] && (
          <div className="mx-6 mt-4 px-4 py-3 rounded-xl flex items-center justify-between bg-blue-surface border border-blue-border">
            <div className="flex items-center gap-2">
              <CheckCircle2 size={16} className="text-green-500" />
              <div>
                <p className="text-xs text-slate-600">Currently selected</p>
                <p className="text-sm text-blue-300">
                  {selectedComponents[activeCategory]!.name}
                </p>
              </div>
            </div>
            <button
              onClick={() => deselectComponent(activeCategory)}
              className="flex items-center gap-1.5 text-xs px-3 py-1.5 rounded-lg transition-all hover:opacity-80 bg-red-remove text-red-400 border border-red-border"
            >
              <Trash2 size={12} />
              Remove
            </button>
          </div>
        )}

        {/* Component grid */}
        <div className="flex-1 overflow-y-auto px-6 py-4">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-sm text-slate-500">
              {categories.find(c => c.id === activeCategory)?.name} — {activeCategoryComponents.length} options
            </h2>
            <div className="flex items-center gap-4 text-xs text-gray-700">
              <span className="flex items-center gap-1">
                <span className="w-2 h-2 rounded-full inline-block bg-green-500" />
                Compatible
              </span>
              <span className="flex items-center gap-1 opacity-50">
                <span className="w-2 h-2 rounded-full inline-block bg-red-500" />
                Incompatible
              </span>
            </div>
          </div>

          <div className="grid grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4 gap-3">
            {activeCategoryComponents.map(component => {
              const { compatible, reasons } = checkCompatibility(component, selectedComponents);
              const isSelected = selectedComponents[activeCategory]?.id === component.id;
              return (
                <ComponentCard
                  key={component.id}
                  component={component}
                  isSelected={isSelected}
                  isCompatible={compatible || isSelected}
                  incompatibilityReasons={reasons}
                  onSelect={() => handleSelect(component)}
                />
              );
            })}
          </div>
        </div>
      </div>

      {/* ───────────────────── RIGHT PANEL ───────────────────── */}
      <aside className="w-80 flex-shrink-0 flex flex-col overflow-y-auto border-l border-line bg-surface-panel">
        <div className="p-5 flex flex-col gap-4 flex-1">
          <h2 className="text-sm font-medium text-slate-400">Build Overview</h2>

          {/* Component list */}
          <div className="flex flex-col gap-2">
            {categories.map(cat => {
              const sel = selectedComponents[cat.id];
              return (
                <div
                  key={cat.id}
                  className={`flex items-center justify-between px-3 py-2.5 rounded-lg border ${
                    sel ? 'bg-surface-tooltip border-blue-selected' : 'bg-surface border-surface-raised'
                  }`}
                >
                  <div className="flex items-center gap-2 overflow-hidden">
                    <span className={sel ? 'text-blue-400' : 'text-gray-700'}>
                      {categoryIcons[cat.id]}
                    </span>
                    <div className="overflow-hidden">
                      <p className="text-xs text-gray-700">{cat.name}</p>
                      {sel ? (
                        <p className="text-xs truncate text-slate-400">{sel.name}</p>
                      ) : (
                        <p className="text-xs text-gray-800">Not selected</p>
                      )}
                    </div>
                  </div>
                  {sel ? (
                    <span className="text-xs flex-shrink-0 ml-2 text-blue-400">
                      ${sel.price.toLocaleString()}
                    </span>
                  ) : (
                    <button
                      onClick={() => setActiveCategory(cat.id)}
                      className="text-xs flex items-center gap-0.5 flex-shrink-0 ml-2 transition-all hover:opacity-80 text-blue-border"
                    >
                      Add <ChevronRight size={11} />
                    </button>
                  )}
                </div>
              );
            })}
          </div>

          {/* Power draw */}
          {(totalPower > 0 || psuWattage > 0) && (
            <div className="rounded-xl p-4 bg-surface border border-line">
              <div className="flex justify-between items-center mb-2">
                <span className="text-xs text-slate-500">Power Draw</span>
                <span className={`text-xs ${psuWattage > 0 && totalPower > psuWattage ? 'text-red-400' : 'text-slate-400'}`}>
                  {totalPower}W{psuWattage > 0 ? ` / ${psuWattage}W` : ''}
                </span>
              </div>
              <div className="h-2 rounded-full overflow-hidden bg-line">
                <div
                  className={`h-full rounded-full transition-all duration-300 ${
                    powerPercent > 90 ? 'bg-red-500' : powerPercent > 70 ? 'bg-amber-500' : 'bg-green-500'
                  }`}
                  style={{ width: psuWattage > 0 ? `${powerPercent}%` : '0%' }}
                />
              </div>
              {!selectedComponents['psu'] && totalPower > 0 && (
                <p className="text-xs mt-1.5 text-slate-600">Select a PSU to validate power</p>
              )}
            </div>
          )}

          {/* Compatibility warnings */}
          {allCompatibilityIssues.length > 0 && (
            <div className="rounded-xl p-3 bg-amber-surface border border-amber-border">
              <div className="flex items-center gap-1.5 mb-2">
                <AlertTriangle size={13} className="text-amber-500" />
                <span className="text-xs text-amber-500">Compatibility Issues</span>
              </div>
              <ul className="space-y-1">
                {allCompatibilityIssues.slice(0, 3).map((issue, i) => (
                  <li key={i} className="text-xs text-amber-900">• {issue}</li>
                ))}
              </ul>
            </div>
          )}

          <div className="mt-auto pt-2 space-y-3">
            {/* Total price */}
            <div className="rounded-xl px-4 py-3 flex items-center justify-between bg-surface border border-line">
              <span className="text-sm text-slate-500">Total Price</span>
              <span className="font-semibold text-slate-200">${totalPrice.toLocaleString()}</span>
            </div>

            {/* Order button */}
            <button
              onClick={() => navigate('/confirm-order')}
              disabled={selectedCount === 0}
              className={`w-full py-3 rounded-xl text-sm font-medium flex items-center justify-center gap-2 transition-all duration-150 ${
                selectedCount > 0
                  ? 'bg-gradient-to-br from-blue-700 to-indigo-600 text-indigo-100 shadow-[0_0_20px_rgba(59,130,246,0.25)]'
                  : 'bg-surface-raised text-gray-700 cursor-not-allowed'
              }`}
            >
              Proceed to Order
              <ChevronRight size={16} />
            </button>

            {selectedCount === 0 && (
              <p className="text-xs text-center text-gray-700">
                Select at least one component to continue
              </p>
            )}
          </div>
        </div>
      </aside>
    </div>
  );
}
