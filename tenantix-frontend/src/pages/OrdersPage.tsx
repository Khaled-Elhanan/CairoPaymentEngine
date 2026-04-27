import { useEffect, useMemo, useState } from "react";
import type { AxiosError } from "axios";
import toast from "react-hot-toast";
import { Alert } from "../components/ui/Alert";
import { FormField } from "../components/forms/FormField";
import { Button } from "../components/ui/Button";
import { Input } from "../components/ui/Input";
import { Spinner } from "../components/ui/Spinner";
import { confirmStripePayment, createOrder, getOrderById, initiateOrderPayment } from "../features/orders/ordersService";

type OrderStatus = "Pending" | "Paid" | "Failed" | "Refunded" | string;
type Gateway = "stripe" | "paymob";

function getErrorMessage(error: unknown, fallback: string) {
  const axiosError = error as AxiosError<{ Error?: string; message?: string }>;
  if (axiosError.response?.data?.Error) {
    return axiosError.response.data.Error;
  }
  if (axiosError.response?.data?.message) {
    return axiosError.response.data.message;
  }
  const genericMessage = (error as { message?: string })?.message;
  return genericMessage ?? fallback;
}

export function OrdersPage() {
  const [amount, setAmount] = useState("220");
  const [currency, setCurrency] = useState("USD");
  const [gateway, setGateway] = useState<Gateway>("stripe");
  const [orderId, setOrderId] = useState("");
  const [externalId, setExternalId] = useState("");
  const [paymentUrl, setPaymentUrl] = useState("");
  const [status, setStatus] = useState<OrderStatus>("Pending");
  const [alreadyConfirmed, setAlreadyConfirmed] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const [createLoading, setCreateLoading] = useState(false);
  const [initiateLoading, setInitiateLoading] = useState(false);
  const [confirmLoading, setConfirmLoading] = useState(false);
  const [statusLoading, setStatusLoading] = useState(false);
  const [openingIframe, setOpeningIframe] = useState(false);
  const [awaitingPaymobReturn, setAwaitingPaymobReturn] = useState(false);

  const canInitiate = Boolean(orderId);
  const canConfirm = Boolean(orderId && externalId);
  const canLoadStatus = canConfirm;
  const isWorkflowLocked = openingIframe;
  const isPaid = status.toLowerCase() === "paid";
  const isStripe = gateway === "stripe";

  const statusBadgeClass = useMemo(() => {
    if (status.toLowerCase() === "paid") {
      return "border border-emerald-200 bg-emerald-50 text-emerald-700";
    }
    if (status.toLowerCase() === "pending") {
      return "border border-amber-200 bg-amber-50 text-amber-700";
    }
    return "border border-slate-200 bg-slate-50 text-slate-700";
  }, [status]);

  const stepThreeDescription = isStripe
    ? "Submit webhook confirmation using the stored external ID."
    : "Complete payment using Paymob secure hosted payment page.";

  useEffect(() => {
    if (gateway === "paymob" && currency !== "EGP") {
      setCurrency("EGP");
    }
  }, [gateway, currency]);

  useEffect(() => {
    if (!awaitingPaymobReturn || !orderId) {
      return;
    }

    const onWindowFocus = async () => {
      setStatusLoading(true);
      setErrorMessage("");
      try {
        await refreshOrderStatus(true);
      } catch (error) {
        const message = getErrorMessage(error, "Failed to load payment status");
        setErrorMessage(message);
        toast.error(message);
      } finally {
        setStatusLoading(false);
      }
    };

    window.addEventListener("focus", onWindowFocus);
    return () => {
      window.removeEventListener("focus", onWindowFocus);
    };
  }, [awaitingPaymobReturn, orderId]);

  async function refreshOrderStatus(showCompletedToast = true) {
    const response = await getOrderById(orderId);
    const latestStatus = String(response.status ?? "Pending");
    setStatus(latestStatus);
    if (latestStatus.toLowerCase() === "paid") {
      setAlreadyConfirmed(true);
      setAwaitingPaymobReturn(false);
      if (showCompletedToast) {
        toast.success("Payment completed successfully");
      }
      return;
    }
    setAlreadyConfirmed(false);
    if (showCompletedToast) {
      toast.success("Payment status refreshed");
    }
  }

  async function handleCreateOrder() {
    setErrorMessage("");
    const parsedAmount = Number(amount);
    if (!Number.isFinite(parsedAmount) || parsedAmount <= 0) {
      setErrorMessage("Amount must be greater than 0.");
      return;
    }

    setCreateLoading(true);
    try {
      const response = await createOrder({
        amount: parsedAmount,
        currency: currency.trim().toUpperCase(),
      });
      setOrderId(String(response.orderId));
      setExternalId("");
      setPaymentUrl("");
      setStatus(String(response.status ?? "Pending"));
      setAlreadyConfirmed(false);
      setAwaitingPaymobReturn(false);
      toast.success("Order created successfully");
    } catch (error) {
      const message = getErrorMessage(error, "Failed to create order");
      setErrorMessage(message);
      toast.error(message);
    } finally {
      setCreateLoading(false);
    }
  }

  async function handleInitiatePayment() {
    if (!canInitiate) {
      return;
    }

    setErrorMessage("");
    setInitiateLoading(true);
    try {
      const response = await initiateOrderPayment(orderId, { gateway });
      setExternalId(String(response.externalId));
      setPaymentUrl(String(response.paymentUrl ?? ""));
      setAlreadyConfirmed(false);
      setAwaitingPaymobReturn(false);
      if (gateway === "paymob") {
        toast.success("Paymob payment token generated");
      } else {
        toast.success("Stripe payment initiated");
      }
    } catch (error) {
      const message = getErrorMessage(error, "Failed to initiate payment");
      setErrorMessage(message);
      toast.error(message);
    } finally {
      setInitiateLoading(false);
    }
  }

  async function handleConfirmPayment() {
    if (!canConfirm || isPaid || !isStripe || isWorkflowLocked) {
      return;
    }

    setErrorMessage("");
    setConfirmLoading(true);
    try {
      const latestOrder = await getOrderById(orderId);
      const latestStatus = String(latestOrder.status ?? "Pending");
      setStatus(latestStatus);
      if (latestStatus.toLowerCase() === "paid") {
        setAlreadyConfirmed(true);
        toast.success("Payment already confirmed");
        return;
      }

      await confirmStripePayment({
        externalId,
        eventId: `manual-confirm-${Date.now()}`,
        gateway: "stripe",
      });
      toast.success("Payment confirmation submitted");
    } catch (error) {
      const backendMessage = getErrorMessage(error, "Failed to confirm payment");
      const isAlreadyConfirmedError =
        backendMessage.toLowerCase().includes("not in a payable state") ||
        backendMessage.toLowerCase().includes("already paid");
      const message = isAlreadyConfirmedError ? "Payment already confirmed" : backendMessage;

      if (isAlreadyConfirmedError) {
        setAlreadyConfirmed(true);
        setStatus("Paid");
        setErrorMessage("");
        toast.success(message);
      } else {
        setErrorMessage(message);
        toast.error(message);
      }
    } finally {
      setConfirmLoading(false);
    }
  }

  function openPaymobPaymentPage() {
    if (!canConfirm || isStripe || isPaid || isWorkflowLocked) {
      return;
    }

    setErrorMessage("");
    setOpeningIframe(true);
    try {
      if (!paymentUrl) {
        throw new Error("Paymob payment URL is missing from backend response.");
      }
      setAwaitingPaymobReturn(true);
      const newWindow = window.open(paymentUrl, "_blank", "noopener,noreferrer");
      if (!newWindow) {
        window.location.href = paymentUrl;
      }
      toast.success("Redirecting to Paymob secure payment page");
    } finally {
      setOpeningIframe(false);
    }
  }

  async function handleLoadStatus() {
    if (!canLoadStatus || isWorkflowLocked) {
      return;
    }

    setErrorMessage("");
    setStatusLoading(true);
    try {
      await refreshOrderStatus(true);
    } catch (error) {
      const message = getErrorMessage(error, "Failed to load payment status");
      setErrorMessage(message);
      toast.error(message);
    } finally {
      setStatusLoading(false);
    }
  }

  return (
    <div id="orders" className="mx-auto w-full max-w-2xl">
      <div className="glass-card border border-gray-100 p-5 md:p-7">
        {errorMessage ? <Alert message={errorMessage} /> : null}

        <div className="mt-4 space-y-6">
          <section className="relative rounded-2xl border border-gray-100 bg-white/90 p-4 shadow-sm md:p-5">
            <div className="absolute left-6 top-14 hidden h-[calc(100%+1.75rem)] w-px bg-gray-200 md:block" />
            <div className="flex items-start gap-3">
              <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-indigo-600 text-sm font-bold text-white">1</div>
              <div className="w-full space-y-4">
                <div>
                  <h2 className="text-lg font-semibold text-gray-900">Create Order</h2>
                  <p className="text-sm text-gray-500">Enter order amount and currency to begin the workflow.</p>
                </div>
                <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
                  <FormField label="Amount" htmlFor="amount">
                    <Input id="amount" value={amount} onChange={(e) => setAmount(e.target.value)} type="number" min="1" disabled={isWorkflowLocked} />
                  </FormField>
                  <FormField label="Currency" htmlFor="currency">
                    <Input
                      id="currency"
                      value={currency}
                      onChange={(e) => setCurrency(e.target.value.toUpperCase())}
                      maxLength={3}
                      disabled={isWorkflowLocked || gateway === "paymob"}
                    />
                  </FormField>
                </div>
                {gateway === "paymob" ? (
                  <p className="text-xs font-medium text-amber-700">Paymob payments currently require EGP currency.</p>
                ) : null}
                <Button type="button" onClick={handleCreateOrder} disabled={createLoading || isWorkflowLocked}>
                  <span className="inline-flex items-center justify-center gap-2">
                    {createLoading ? <Spinner className="h-4 w-4 border-white/50 border-t-white" /> : null}
                    {createLoading ? "Creating Order..." : "Create Order"}
                  </span>
                </Button>
                {orderId ? (
                  <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
                    <p className="font-semibold">Order created successfully</p>
                    <p className="mt-1 break-all font-mono text-xs">Order ID: {orderId}</p>
                  </div>
                ) : null}
              </div>
            </div>
          </section>

          <section className="relative rounded-2xl border border-gray-100 bg-white/90 p-4 shadow-sm md:p-5">
            <div className="absolute left-6 top-14 hidden h-[calc(100%+1.75rem)] w-px bg-gray-200 md:block" />
            <div className="flex items-start gap-3">
              <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-indigo-600 text-sm font-bold text-white">2</div>
              <div className="w-full space-y-4">
                <div>
                  <h2 className="text-lg font-semibold text-gray-900">Initiate Payment</h2>
                  <p className="text-sm text-gray-500">Create a payment session for the selected gateway and active order.</p>
                </div>
                <FormField label="Gateway" htmlFor="gateway">
                  <select
                    id="gateway"
                    value={gateway}
                    onChange={(e) => setGateway(e.target.value as Gateway)}
                    disabled={isWorkflowLocked}
                    className="w-full rounded-xl border border-gray-200 bg-white px-3 py-3 text-sm text-gray-800 shadow-sm outline-none transition-all focus:border-indigo-300 focus:ring-2 focus:ring-indigo-400 disabled:cursor-not-allowed disabled:opacity-60"
                  >
                    <option value="stripe">Stripe</option>
                    <option value="paymob">Paymob</option>
                  </select>
                </FormField>
                <Button type="button" onClick={handleInitiatePayment} disabled={!canInitiate || initiateLoading || isWorkflowLocked}>
                  <span className="inline-flex items-center justify-center gap-2">
                    {initiateLoading ? <Spinner className="h-4 w-4 border-white/50 border-t-white" /> : null}
                    {initiateLoading ? "Initiating..." : `Initiate ${gateway === "stripe" ? "Stripe" : "Paymob"} Payment`}
                  </span>
                </Button>
                {externalId ? (
                  <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
                    <p className="font-semibold">Payment intent generated</p>
                    <p className="mt-1 flex flex-wrap items-center gap-2 break-all font-mono text-xs">
                      <span className="rounded-full border border-emerald-300 bg-emerald-100 px-2 py-0.5 text-[10px] font-semibold uppercase">
                        {gateway}
                      </span>
                      <span>External ID: {externalId}</span>
                    </p>
                  </div>
                ) : null}
              </div>
            </div>
          </section>

          <section className="relative rounded-2xl border border-gray-100 bg-white/90 p-4 shadow-sm md:p-5">
            <div className="absolute left-6 top-14 hidden h-[calc(100%+1.75rem)] w-px bg-gray-200 md:block" />
            <div className="flex items-start gap-3">
              <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-indigo-600 text-sm font-bold text-white">3</div>
              <div className="w-full space-y-4">
                <div>
                  <h2 className="text-lg font-semibold text-gray-900">Confirm Payment</h2>
                  <p className="text-sm text-gray-500">{stepThreeDescription}</p>
                </div>
                {isStripe ? (
                  <Button type="button" onClick={handleConfirmPayment} disabled={!canConfirm || confirmLoading || isPaid || isWorkflowLocked}>
                    <span className="inline-flex items-center justify-center gap-2">
                      {confirmLoading ? <Spinner className="h-4 w-4 border-white/50 border-t-white" /> : null}
                      {confirmLoading ? "Confirming..." : "Confirm Payment"}
                    </span>
                  </Button>
                ) : (
                  <Button type="button" onClick={openPaymobPaymentPage} disabled={!canConfirm || isPaid || isWorkflowLocked || openingIframe}>
                    <span className="inline-flex items-center justify-center gap-2">
                      {openingIframe ? <Spinner className="h-4 w-4 border-white/50 border-t-white" /> : null}
                      {openingIframe ? "Redirecting..." : "Pay with Paymob"}
                    </span>
                  </Button>
                )}
                {!isStripe ? (
                  <p className="text-xs text-gray-500">You will be redirected to Paymob secure payment page.</p>
                ) : null}
                {alreadyConfirmed ? (
                  <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
                    <p className="font-semibold">Payment already confirmed</p>
                  </div>
                ) : null}
              </div>
            </div>
          </section>

          <section className="rounded-2xl border border-gray-100 bg-white/90 p-4 shadow-sm md:p-5">
            <div className="flex items-start gap-3">
              <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-indigo-600 text-sm font-bold text-white">4</div>
              <div className="w-full space-y-4">
                <div>
                  <h2 className="text-lg font-semibold text-gray-900">Payment Status</h2>
                  <p className="text-sm text-gray-500">Check the latest order status from the backend.</p>
                </div>
                <div className="flex flex-col items-start gap-3 sm:flex-row sm:items-center sm:justify-between">
                  <span className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold ${statusBadgeClass}`}>{status}</span>
                  <Button
                    type="button"
                    className="sm:w-auto sm:min-w-52"
                    onClick={handleLoadStatus}
                    disabled={!canLoadStatus || statusLoading || isWorkflowLocked}
                  >
                    <span className="inline-flex items-center justify-center gap-2">
                      {statusLoading ? <Spinner className="h-4 w-4 border-white/50 border-t-white" /> : null}
                      {statusLoading ? "Checking..." : "Refresh Payment Status"}
                    </span>
                  </Button>
                </div>
              </div>
            </div>
          </section>
        </div>
      </div>
    </div>
  );
}
