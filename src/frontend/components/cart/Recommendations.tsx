import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import Link from "next/link";

export default function Recommendations() {
  return (
    <div className="mt-16">
      <Card>
        <CardHeader>
          <CardTitle>You might also like</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-center py-8">
            <p className="text-muted-foreground mb-4">
              Discover more products that match your style
            </p>
            <Button variant="outline" asChild>
              <Link href="/">Browse Products</Link>
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
