import { Globe, Warehouse, Truck, Wrench, PackageCheck, ChevronRight } from 'lucide-react';
import { useNavigate } from 'react-router';
import { useApp, ORDER_STATUSES } from '../context/AppContext';
import { categories, getTotalPowerDraw } from '../data/components';

// Icons for each status step
const STATUS_ICONS = [
  Globe,        // 0: Order Placed — Website
  Warehouse,    // 1: Components Ready — Warehouse
  Truck,        // 2: Transporting Components — AGV/Transport
  Wrench,       // 3: Building Computer — Assembly
  Truck,        // 4: Transporting Computer — Delivery vehicle
  PackageCheck, // 5: Ready for Delivery
];

// Station labels
const STATION_LABELS = ['Website', 'Warehouse', 'Transport', 'Assembly', 'Transport', 'Delivery'];

function StatusNode({
  index,
  currentStatus,
  isLast,
}: {
  index: number;
  currentStatus: number;
  isLast: boolean;
}) {
  const isDone = index < currentStatus;
  const isCurrent = index === currentStatus;
  const isPending = index > currentStatus;
  const Icon = STATUS_ICONS[index];
  const status = ORDER_STATUSES[index];

  return (
    <div className="flex items-start flex-1 min-w-0">
      <div className="flex flex-col items-center w-full">
        {/* Node */}
        <div
          className={`relative w-16 h-16 rounded-2xl flex items-center justify-center transition-all duration-300 flex-shrink-0 border-2 ${
            isDone
              ? 'bg-green-900 border-green-600'
              : isCurrent
              ? 'bg-blue-900 border-blue-500 shadow-[0_0_24px_rgba(59,130,246,0.35)]'
              : 'bg-surface-raised border-line-card'
          }`}
        >
          <Icon
            size={26}
            className={isDone ? 'text-green-500' : isCurrent ? 'text-blue-300' : 'text-gray-700'}
            strokeWidth={1.8}
          />
          {isCurrent && (
            <span className="absolute -top-1 -right-1 w-3 h-3 rounded-full animate-pulse bg-blue-500" />
          )}
          {isDone && (
            <span className="absolute -top-1 -right-1 w-4 h-4 rounded-full flex items-center justify-center bg-green-600">
              <svg width="9" height="9" viewBox="0 0 9 9" fill="none">
                <path d="M1.5 4.5L3.5 6.5L7.5 2.5" stroke="white" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
              </svg>
            </span>
          )}
        </div>

        {/* Station label */}
        <p className={`text-[10px] mt-2 text-center ${isDone ? 'text-green-600' : isCurrent ? 'text-blue-500' : 'text-gray-700'}`}>
          {STATION_LABELS[index]}
        </p>

        {/* Status label */}
        <p className={`text-xs text-center mt-1 leading-tight ${isDone ? 'text-green-400' : isCurrent ? 'text-blue-300' : 'text-gray-700'}`}>
          {status.label}
        </p>
        <p className={`text-[10px] text-center mt-0.5 leading-tight ${isDone ? 'text-green-800' : isCurrent ? 'text-blue-700' : 'text-line'}`}>
          {status.sublabel}
        </p>

        {/* Status pill */}
        {isCurrent && (
          <span className="mt-2 text-[10px] px-2 py-0.5 rounded-full bg-blue-900 text-blue-300">
            In Progress
          </span>
        )}
        {isDone && (
          <span className="mt-2 text-[10px] px-2 py-0.5 rounded-full bg-green-900 text-green-400">
            Completed
          </span>
        )}
        {isPending && (
          <span className="mt-2 text-[10px] px-2 py-0.5 rounded-full bg-surface-raised text-gray-700">
            Pending
          </span>
        )}
      </div>

      {/* Connector arrow */}
      {!isLast && (
        <div className="flex items-center mt-7 flex-shrink-0 px-1 min-w-[28px]">
          <div
            className={`flex-1 h-0.5 rounded-full transition-all duration-300 ${index < currentStatus ? 'bg-green-600' : 'bg-line'}`}
          />
          <ChevronRight
            size={14}
            className={index < currentStatus ? 'text-green-600' : index === currentStatus ? 'text-blue-500' : 'text-line'}
          />
        </div>
      )}
    </div>
  );
}

export function OrderStatusPage() {
  const { hasActiveOrder, orderInfo, currentOrderId, orderStatus, selectedComponents } = useApp();
  const navigate = useNavigate();

  const selectedList = categories
    .map(cat => ({ cat, component: selectedComponents[cat.id] }))
    .filter(({ component }) => !!component);

  const totalPrice = selectedList.reduce((s, { component }) => s + (component?.price ?? 0), 0);
  const totalPower = getTotalPowerDraw(selectedComponents);

  if (!hasActiveOrder) {
    return (
      <div className="flex items-center justify-center h-[calc(100vh-64px)]">
        <div className="text-center space-y-4">
          <div className="w-16 h-16 mx-auto rounded-2xl flex items-center justify-center bg-surface-card">
            <Truck size={28} className="text-gray-700" />
          </div>
          <h2 className="text-slate-400">No active order</h2>
          <p className="text-sm text-gray-700">
            Configure and confirm an order to track its status.
          </p>
          <button
            onClick={() => navigate('/configure')}
            className="mt-2 px-6 py-2.5 rounded-xl text-sm flex items-center gap-2 mx-auto transition-all hover:opacity-90 bg-blue-700 text-blue-200"
          >
            Configure PC
            <ChevronRight size={14} />
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-screen-xl mx-auto px-6 py-6 h-[calc(100vh-64px)] overflow-y-auto">
      {/* Header */}
      <div className="mb-8">
        <div className="flex items-start justify-between">
          <div>
            <h1 className="text-slate-200">Order Status</h1>
            <p className="text-xs mt-0.5 text-slate-600">
              Order for <span className="text-blue-400">{orderInfo?.name}</span>
              {' · '}
              <span className="text-slate-600">{orderInfo?.email}</span>
            </p>
            {currentOrderId != null && (
              <p className="text-xs mt-1 text-slate-500">Order ID: #{currentOrderId}</p>
            )}
          </div>
        </div>
      </div>

      {/* ─── Status tracker ─── */}
      <div className="rounded-2xl p-8 mb-6 bg-surface-card border border-line-card">
        <div className="flex items-start justify-between mb-6">
          <div>
            <h2 className="text-sm text-slate-400">Tracking Progress</h2>
            <p className="text-xs mt-0.5 text-gray-700">
              {ORDER_STATUSES[orderStatus].label}
            </p>
          </div>
          <div className="flex items-center gap-2">
            <div className="flex items-center gap-1.5 text-xs px-3 py-1.5 rounded-full bg-green-subtle text-green-500 border border-green-900">
              <span className="w-1.5 h-1.5 rounded-full animate-pulse bg-green-500" />
              Live Tracking
            </div>
          </div>
        </div>

        {/* Progress bar */}
        <div className="mb-8">
          <div className="flex justify-between text-xs mb-2 text-gray-700">
            <span>Progress</span>
            <span className="text-slate-500">{orderStatus + 1} / {ORDER_STATUSES.length}</span>
          </div>
          <div className="h-1.5 rounded-full overflow-hidden bg-line">
            <div
              className="h-full rounded-full transition-all duration-700 bg-gradient-to-r from-blue-700 to-green-500"
              style={{ width: `${((orderStatus) / (ORDER_STATUSES.length - 1)) * 100}%` }}
            />
          </div>
        </div>

        {/* Node track */}
        <div className="flex items-start overflow-x-auto pb-2">
          {ORDER_STATUSES.map((_, index) => (
            <StatusNode
              key={index}
              index={index}
              currentStatus={orderStatus}
              isLast={index === ORDER_STATUSES.length - 1}
            />
          ))}
        </div>
      </div>

      {/* ─── Two-column bottom ─── */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Order details */}
        <div className="rounded-2xl p-5 bg-surface-card border border-line-card">
          <h3 className="text-sm mb-4 text-slate-400">Order Summary</h3>
          <div className="space-y-2">
            {selectedList.map(({ cat, component }) =>
              component ? (
                <div key={cat.id} className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <div>
                      <p className="text-xs text-slate-600">{cat.name}</p>
                      <p className="text-xs text-slate-400">{component.name}</p>
                    </div>
                  </div>
                  <span className="text-xs text-blue-400">
                    ${component.price.toLocaleString()}
                  </span>
                </div>
              ) : null
            )}
          </div>
          <div className="flex justify-between items-center pt-3 mt-3 border-t border-line-card">
            <span className="text-sm text-slate-500">Total</span>
            <span className="font-semibold text-slate-200">${totalPrice.toLocaleString()}</span>
          </div>
          <div className="flex justify-between mt-1">
            <span className="text-xs text-gray-700">Power Draw</span>
            <span className="text-xs text-slate-500">{totalPower}W</span>
          </div>
        </div>

        {/* Delivery info */}
        <div className="rounded-2xl p-5 bg-surface-card border border-line-card">
          <h3 className="text-sm mb-4 text-slate-400">Delivery Information</h3>
          <div className="space-y-4">
            <div>
              <p className="text-xs mb-1 text-gray-700">Recipient</p>
              <p className="text-sm text-slate-200">{orderInfo?.name}</p>
            </div>
            <div>
              <p className="text-xs mb-1 text-gray-700">Email</p>
              <p className="text-sm text-slate-400">{orderInfo?.email}</p>
            </div>
            <div>
              <p className="text-xs mb-1 text-gray-700">Delivery Address</p>
              <p className="text-sm text-slate-400">{orderInfo?.address}</p>
            </div>
          </div>

          {/* Next steps */}
          <div className="mt-4 rounded-xl p-3 bg-surface border border-line">
            <p className="text-xs mb-2 text-gray-700">Next Steps</p>
            {ORDER_STATUSES.slice(orderStatus).map((status, i) => (
              <div key={status.id} className="flex items-center gap-2 mb-1">
                <span className={`w-1.5 h-1.5 rounded-full flex-shrink-0 ${i === 0 ? 'bg-blue-500' : 'bg-line'}`} />
                <p className={`text-xs ${i === 0 ? 'text-blue-300' : 'text-gray-700'}`}>
                  {status.label}
                </p>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
