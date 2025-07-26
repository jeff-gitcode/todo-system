'use client';

import { Card } from '@/components/ui/card';

export default function GlobalError({ error, reset }: { error: Error; reset: () => void }) {
    return (
        <Card className="p-6 text-center text-red-600">
            <h2 className="text-xl font-bold mb-2">Something went wrong!</h2>
            <div className="mb-4">{error.message}</div>
            <button
                className="bg-red-500 text-white px-4 py-2 rounded"
                onClick={() => reset()}
            >
                Try again
            </button>
        </Card>
    );
}