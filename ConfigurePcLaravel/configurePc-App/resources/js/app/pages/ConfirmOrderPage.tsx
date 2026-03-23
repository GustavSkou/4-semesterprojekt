import { useState } from 'react';
import type { ReactNode } from 'react';
import {
  Edit2, Trash2, ChevronRight,
  ShoppingCart, AlertTriangle, CheckCircle2, User, Mail, MapPin,
} from 'lucide-react';
import { useNavigate } from 'react-router';
import { categories, getTotalPowerDraw } from '../data/components';
import { useApp, OrderInfo } from '../context/AppContext';
import { categoryIcons } from '../constants/categoryIcons';

function InputField({
  icon, label, type = 'text', value, onChange, placeholder, error,
}: {
  icon: ReactNode;
  label: string;
  type?: string;
  value: string;
  onChange: (v: string) => void;
  placeholder: string;
  error?: string;
}) {
  const [focused, setFocused] = useState(false);
  return (
    <div>
      <label className="block text-xs mb-1.5 text-slate-500">{label}</label>
      <div
        className={`flex items-center gap-3 px-4 py-3 rounded-xl transition-all bg-surface-card ${
          focused
            ? 'border-[1.5px] border-blue-500'
            : error
            ? 'border-[1.5px] border-red-500'
            : 'border border-line-card'
        }`}
      >
        <span className={focused ? 'text-blue-400' : 'text-gray-700'}>{icon}</span>
        <input
          type={type}
          value={value}
          onChange={e => onChange(e.target.value)}
          onFocus={() => setFocused(true)}
          onBlur={() => setFocused(false)}
          placeholder={placeholder}
          className="flex-1 bg-transparent outline-none text-sm text-slate-200 caret-blue-500"
        />
      </div>
      {error && (
        <p className="text-xs mt-1 text-red-400">{error}</p>
      )}
    </div>
  );
}

export function ConfirmOrderPage() {
  const { selectedComponents, clearConfiguration, placeOrder } = useApp();
  const navigate = useNavigate();

  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [address, setAddress] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  const selectedList = categories
    .map(cat => ({ cat, component: selectedComponents[cat.id] }))
    .filter(({ component }) => !!component);

  const totalPrice = selectedList.reduce((sum, { component }) => sum + (component?.price ?? 0), 0);
  const totalPower = getTotalPowerDraw(selectedComponents);
  const psuWattage = selectedComponents['psu']?.psuWattage ?? 0;
  const powerOk = psuWattage === 0 || totalPower <= psuWattage;

  const validate = () => {
    const errs: Record<string, string> = {};
    if (!name.trim()) errs.name = 'Name is required';
    if (!email.trim()) errs.email = 'Email is required';
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) errs.email = 'Enter a valid email';
    if (!address.trim()) errs.address = 'Address is required';
    return errs;
  };

  const handleConfirmOrder = async () => {
    const errs = validate();
    if (Object.keys(errs).length > 0) {
      setErrors(errs);
      return;
    }
    const info: OrderInfo = { name: name.trim(), email: email.trim(), address: address.trim() };
    await placeOrder(info);
    navigate('/order-status');
  };

  const handleDelete = () => {
    clearConfiguration();
    setShowDeleteConfirm(false);
    navigate('/configure');
  };

  if (selectedList.length === 0) {
    return (
      <div className="flex items-center justify-center h-[calc(100vh-64px)]">
        <div className="text-center space-y-4">
          <div className="w-16 h-16 mx-auto rounded-2xl flex items-center justify-center bg-surface-card">
            <ShoppingCart size={28} className="text-gray-700" />
          </div>
          <h2 className="text-lg text-slate-400">No components selected</h2>
          <p className="text-sm text-gray-700">
            Go back to the Configure tab and select your components.
          </p>
          <button
            onClick={() => navigate('/configure')}
            className="mt-2 px-6 py-2.5 rounded-xl text-sm flex items-center gap-2 mx-auto transition-all hover:opacity-90 bg-blue-700 text-blue-200"
          >
            <Edit2 size={14} />
            Configure PC
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-screen-xl mx-auto px-6 py-6 flex gap-6 h-[calc(100vh-64px)] overflow-hidden">
      {/* ─── Left: Build Summary ─────────────────────────────── */}
      <div className="flex-1 flex flex-col overflow-hidden">
        <div className="flex items-center justify-between mb-5">
          <div>
            <h1 className="text-slate-200">Confirm Your Order</h1>
            <p className="text-xs mt-0.5 text-slate-600">
              Review your configuration before placing the order
            </p>
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => navigate('/configure')}
              className="flex items-center gap-1.5 text-sm px-4 py-2 rounded-xl transition-all hover:opacity-80 bg-line text-blue-300 border border-blue-border"
            >
              <Edit2 size={14} />
              Edit Configuration
            </button>
            <button
              onClick={() => setShowDeleteConfirm(true)}
              className="flex items-center gap-1.5 text-sm px-4 py-2 rounded-xl transition-all hover:opacity-80 bg-red-surface text-red-400 border border-red-border"
            >
              <Trash2 size={14} />
              Delete Build
            </button>
          </div>
        </div>

        {/* Delete confirm modal */}
        {showDeleteConfirm && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70">
            <div className="rounded-2xl p-6 w-80 bg-surface-card border border-line-card">
              <div className="w-12 h-12 rounded-xl flex items-center justify-center mx-auto mb-4 bg-red-surface">
                <Trash2 size={22} className="text-red-400" />
              </div>
              <h3 className="text-center mb-1 text-slate-200">Delete Build?</h3>
              <p className="text-xs text-center mb-6 text-slate-600">
                All selected components will be removed and you'll be taken back to Configure.
              </p>
              <div className="flex gap-3">
                <button
                  onClick={() => setShowDeleteConfirm(false)}
                  className="flex-1 py-2.5 rounded-xl text-sm transition-all hover:opacity-80 bg-line text-slate-400 border border-line-card"
                >
                  Cancel
                </button>
                <button
                  onClick={handleDelete}
                  className="flex-1 py-2.5 rounded-xl text-sm transition-all hover:opacity-80 bg-red-900 text-red-300"
                >
                  Delete
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Component cards */}
        <div className="flex-1 overflow-y-auto space-y-2 pr-1">
          {selectedList.map(({ cat, component }) => {
            if (!component) return null;
            return (
              <div
                key={cat.id}
                className="rounded-xl px-5 py-4 flex items-start gap-4 bg-surface-card border border-line-card"
              >
                <div className="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0 bg-blue-selected">
                  <span className="text-blue-400">{categoryIcons[cat.id]}</span>
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <p className="text-xs mb-0.5 text-gray-700">{cat.name}</p>
                      <p className="text-sm text-slate-200">{component.name}</p>
                      <p className="text-xs mt-0.5 text-slate-600">{component.brand}</p>
                    </div>
                    <span className="font-medium text-sm flex-shrink-0 text-blue-400">
                      ${component.price.toLocaleString()}
                    </span>
                  </div>

                  {/* Specs row */}
                  <div className="flex flex-wrap gap-x-4 gap-y-1 mt-2">
                    {component.specs.slice(0, 4).map((spec, i) => (
                      <div key={spec.label + i} className="flex items-center gap-1">
                        <span className="text-[10px] text-gray-700">{spec.label}:</span>
                        <span className="text-[10px] text-slate-500">{spec.value}</span>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            );
          })}
        </div>

        {/* Power warning */}
        {!powerOk && (
          <div className="mt-3 rounded-xl p-3 flex items-center gap-2 bg-amber-surface border border-amber-border">
            <AlertTriangle size={14} className="text-amber-500" />
            <p className="text-xs text-amber-800">
              Power warning: Build draws {totalPower}W but PSU is rated for {psuWattage}W.
            </p>
          </div>
        )}
      </div>

      {/* ─── Right: Customer Info + Price ────────────────────── */}
      <aside className="w-80 flex-shrink-0 flex flex-col gap-4 min-h-0">
        {/* Price summary */}
        <div className="rounded-2xl p-5 bg-surface-card border border-line-card">
          <h3 className="text-sm mb-4 text-slate-400">Price Summary</h3>
          <div className="space-y-2 mb-4">
            {selectedList.map(({ cat, component }) =>
              component ? (
                <div key={cat.id} className="flex justify-between text-xs">
                  <span className="text-slate-600">{cat.name}</span>
                  <span className="text-slate-500">${component.price.toLocaleString()}</span>
                </div>
              ) : null
            )}
          </div>
          <div className="flex justify-between items-center pt-3 border-t border-line-card">
            <span className="text-sm text-slate-400">Total</span>
            <span className="font-semibold text-slate-200">${totalPrice.toLocaleString()}</span>
          </div>
          <div className="flex justify-between items-center mt-1">
            <span className="text-xs text-gray-700">Power Draw</span>
            <span className={`text-xs ${powerOk ? 'text-green-500' : 'text-red-400'}`}>
              {totalPower}W{psuWattage > 0 && ` / ${psuWattage}W PSU`}
            </span>
          </div>
        </div>

        {/* Customer info form */}
        <div className="rounded-2xl p-5 flex flex-col gap-4 flex-1 bg-surface-card border border-line-card">
          <h3 className="text-sm text-slate-400">Customer Information</h3>

          <InputField
            icon={<User size={15} />}
            label="Full Name"
            value={name}
            onChange={v => { setName(v); setErrors(e => ({ ...e, name: '' })); }}
            placeholder="Jane Doe"
            error={errors.name}
          />
          <InputField
            icon={<Mail size={15} />}
            label="Email Address"
            type="email"
            value={email}
            onChange={v => { setEmail(v); setErrors(e => ({ ...e, email: '' })); }}
            placeholder="jane@example.com"
            error={errors.email}
          />
          <InputField
            icon={<MapPin size={15} />}
            label="Delivery Address"
            value={address}
            onChange={v => { setAddress(v); setErrors(e => ({ ...e, address: '' })); }}
            placeholder="123 Main Street, City"
            error={errors.address}
          />

          <button
            onClick={handleConfirmOrder}
            className="w-full py-3 rounded-xl text-sm font-medium flex items-center justify-center gap-2 mt-auto transition-all hover:opacity-90 bg-gradient-to-br from-blue-700 to-indigo-600 text-indigo-100 shadow-[0_0_20px_rgba(59,130,246,0.2)]"
          >
            <CheckCircle2 size={16} />
            Confirm Order
            <ChevronRight size={14} />
          </button>
        </div>
      </aside>
    </div>
  );
}
